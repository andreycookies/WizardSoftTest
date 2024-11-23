using Domain.Interfaces;
using Domain.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrasctructure.Repositories
{
    public class RedisNodeRepository : INodeRepository
    {
        private readonly IDatabase _database;
        private const string NodesKey = "nodes";

        public RedisNodeRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task<List<NodeView>> GetTreeNodesAsync(Guid? id = null)
        {
            var result = new List<NodeView>();

            var nodes = await _database.HashValuesAsync(NodesKey);
            foreach (var node in nodes) 
            {
                if (!node.HasValue)
                    continue;

                var currentNode = JsonSerializer.Deserialize<Node>(node!);
                if (currentNode == null) 
                    continue;

                if (currentNode.ParentId != id)
                    continue;

                result.Add(new NodeView
                {
                    Id = currentNode.Id,
                    Body = currentNode.Body,
                    Children = await GetTreeNodesAsync(currentNode.Id)
                });
            }

            return result;
        }

        public async Task CreateNodeAsync(Node node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            if (await _database.HashExistsAsync(NodesKey, node.Id.ToString()))
            {
                throw new InvalidOperationException($"Node with ID {node.Id} already exists.");
            }

            if (node.ParentId != null && !(await _database.HashExistsAsync(NodesKey, node.ParentId.ToString())))
            {
                throw new InvalidOperationException($"Parent with ID {node.ParentId} is not exists.");
            }

            await SaveNodeAsync(node);

            if (node.ParentId.HasValue)
            {
                var parent = await GetNodeAsync(node.ParentId.Value);

                if (parent != null)
                {
                    parent.Children.Add(node.Id);
                    await UpdateNodeAsync(parent);
                }
            }
        }

        public async Task UpdateNodeAsync(Node node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            if (!await _database.HashExistsAsync(NodesKey, node.Id.ToString()))
            {
                throw new InvalidOperationException($"Node with ID {node.Id} does not exist.");
            }

            await SaveNodeAsync(node);

            if (node.ParentId.HasValue)
            {
                var parent = await GetNodeAsync(node.ParentId.Value);

                if (parent != null && !parent.Children.Contains(node.Id))
                {
                    parent.Children.Add(node.Id);
                    await UpdateNodeAsync(parent);
                }
            }
        }

        public async Task DeleteNodeAsync(Guid nodeId)
        {
            var node = await GetNodeAsync(nodeId);

            if (node != null)
            {
                if (node.ParentId.HasValue)
                {
                    var parent = await GetNodeAsync(node.ParentId.Value);

                    if (parent != null)
                    {
                        parent.Children.Remove(node.Id);
                        await UpdateNodeAsync(parent);
                    }
                }

                await _database.HashDeleteAsync(NodesKey, node.Id.ToString());

                foreach (var childId in node.Children)
                {
                    await DeleteNodeAsync(childId);
                }
            }
        }

        public async Task SaveNodeAsync(Node node)
        {
            string json = JsonSerializer.Serialize(node);
            await _database.HashSetAsync(NodesKey, new StackExchange.Redis.HashEntry[]
            {
                new StackExchange.Redis.HashEntry(node.Id.ToString(), JsonSerializer.Serialize(node))
            });
        }

        public async Task<Node?> GetNodeAsync(Guid id)
        {
            RedisValue value = await _database.HashGetAsync(NodesKey, id.ToString());

            if (!value.HasValue) return null;

            return JsonSerializer.Deserialize<Node>(value);
        }
    }
}
