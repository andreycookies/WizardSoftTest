using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface INodeRepository
    {
        Task<Node?> GetNodeAsync(Guid id);
        Task<List<NodeView>> GetTreeNodesAsync(Guid? id);
        Task CreateNodeAsync(Node node);
        Task UpdateNodeAsync(Node node);
        Task DeleteNodeAsync(Guid id);
    }
}
