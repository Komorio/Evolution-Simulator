namespace Slash.Unity.DataBind.Core.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Slash.Unity.DataBind.Core.Utils;

    /// <summary>
    ///     A data context node which might have additional children.
    /// </summary>
    public class BranchDataNode : DataNode
    {
        private readonly List<IDataNode> children = new List<IDataNode>();

        /// <inheritdoc />
        public BranchDataNode(NodeTypeInfo typeInfo, IDataNode parentNode, string name) : base(typeInfo,
            parentNode, name)
        {
        }

        /// <inheritdoc />
        public BranchDataNode(NodeTypeInfo typeInfo, string name) : base(typeInfo, name)
        {
        }

        public override void Destroy()
        {
            foreach (var child in this.children)
            {
                child.Destroy();
            }
            this.children.Clear();

            base.Destroy();
        }

        /// <inheritdoc />
        public override bool IsMonitored()
        {
            return base.IsMonitored() || this.children.Any();
        }

        /// <inheritdoc />
        public override IDataNode FindDescendant(string path)
        {
            var pointPos = path.IndexOf(DataBindSettings.PathSeparator);
            var nodeName = path;
            string pathRest = null;
            if (pointPos >= 0)
            {
                nodeName = path.Substring(0, pointPos);
                pathRest = path.Substring(pointPos + 1);
            }

            if (nodeName == DataBindSettings.SelfReferencePath)
            {
                return this;
            }

            // Get children with name.
            var childNode = this.GetOrCreateChild(nodeName);
            if (childNode == null)
            {
                return null;
            }

            return string.IsNullOrEmpty(pathRest) ? childNode : childNode.FindDescendant(pathRest);
        }

        /// <inheritdoc />
        public override bool RemoveDescendant(string path)
        {
            var pointPos = path.IndexOf(DataBindSettings.PathSeparator);
            var nodeName = path;
            string pathRest = null;
            if (pointPos >= 0)
            {
                nodeName = path.Substring(0, pointPos);
                pathRest = path.Substring(pointPos + 1);
            }

            if (nodeName == DataBindSettings.SelfReferencePath)
            {
                throw new System.Exception("Can not remove self");
            }

            // Get children with name.
            var childNode = this.GetChild(nodeName);
            if (childNode == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(pathRest))
            {
                childNode.Destroy();
                return this.children.Remove(childNode);
            }

            return childNode.RemoveDescendant(pathRest);
        }

        private IDataNode CreateChild(string name)
        {
            // Get type of child.
            var typeInfo = ReflectionUtils.GetNodeTypeInfo(this.DataType, name);
            if (typeInfo == null)
            {
                // No child with this name.
                return null;
            }

            var childNode = typeInfo.CanHaveChildren
                ? new BranchDataNode(typeInfo, this, name)
                : (IDataNode)new LeafDataNode(typeInfo, this, name);
            this.children.Add(childNode);
            return childNode;
        }

        private IDataNode GetChild(string name)
        {
            return this.children.FirstOrDefault(child => child.Name == name);
        }

        private IDataNode GetOrCreateChild(string name)
        {
            return this.GetChild(name) ?? this.CreateChild(name);
        }
    }
}