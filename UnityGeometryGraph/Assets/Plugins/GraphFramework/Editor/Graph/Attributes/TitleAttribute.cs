using System;

namespace GraphFramework.Editor {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TitleAttribute : Attribute {
        public readonly string[] Title;
        public TitleAttribute(params string[] title) {
            Title = title;
        }
    }
}