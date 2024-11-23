using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Node
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public NodeBody Body { get; set; }
        public Guid? ParentId { get; set; }
        public List<Guid> Children { get; set; } = new();
    }

    public sealed class NodeView
    {
        public Guid Id { get; init; }
        public NodeBody Body { get; init; }
        public List<NodeView> Children { get; init; } = new();
    }

    public class NodeBody
    {
        public string Name { get; set; }
    }
}
