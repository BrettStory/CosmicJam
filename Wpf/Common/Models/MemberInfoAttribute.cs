namespace Macabre2D.Wpf.Common.Models {

    using System;
    using System.Reflection;

    public sealed class MemberInfoAttribute<T> where T : Attribute {

        public MemberInfoAttribute(MemberInfo memberInfo, T attribute) {
            this.MemberInfo = memberInfo;
            this.Attribute = attribute;
        }

        public T Attribute { get; }

        public MemberInfo MemberInfo { get; }
    }
}