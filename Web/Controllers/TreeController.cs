using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TreeController : ControllerBase
    {
        private readonly INodeRepository _repository;

        public TreeController(INodeRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Node>> GetNode(Guid id)
        {
            var node = await _repository.GetNodeAsync(id);
            if (node == null) return NotFound();
            return Ok(node);
        }

        [HttpGet]
        public async Task<ActionResult<Node>> GetTreeNode(Guid? nodeId = null)
        {
            var nodes = await _repository.GetTreeNodesAsync(nodeId);
            return Ok(nodes);
        }

        [HttpPost]
        public async Task<ActionResult<Node>> CreateNode(Node node)
        {
            await _repository.CreateNodeAsync(node);
            return CreatedAtAction(nameof(GetNode), new { id = node.Id }, node);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNode(Guid id, Node updatedNode)
        {
            if (id != updatedNode.Id) return BadRequest();
            await _repository.UpdateNodeAsync(updatedNode);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNode(Guid id)
        {
            var node = await _repository.GetNodeAsync(id);
            if (node == null) return NotFound();
            await _repository.DeleteNodeAsync(id);
            return NoContent();
        }
    }
}
