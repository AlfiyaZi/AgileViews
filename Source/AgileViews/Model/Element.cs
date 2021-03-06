using System;
using System.Diagnostics;

namespace AgileViews.Model
{
    [DebuggerDisplay("{UserData}")]
    public class Element<T> : Element
    {
        public Element Parent { get; set; }

        public T UserData { get; set; }

        public override Element GetParent()
        {
            return Parent;
        }
    }

    public class Element<T1, T2> : Element<T1>
    {
        public T2 UserData2 { get; set; }
    }

    public abstract class Element : Information
    {
        private Guid _guid = Guid.NewGuid();

        internal Element()
        {
        }

        public Model Model { get; set; }

        /// <summary>
        ///     Element name which will be displayed in diagrams
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Longer description of the element, which can be added in notes or underneath diagrams.
        /// </summary>
        public string Description { get; set; }

        public string Alias
        {
            get { return Name.Replace(" ", ""); }
        }

        public abstract Element GetParent();

        public Relationship Uses(Element target, string description)
        {
            return Model.AddRelationship(this, target, description);
        }
    }
}