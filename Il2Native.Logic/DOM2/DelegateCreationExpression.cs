﻿namespace Il2Native.Logic.DOM2
{
    using System.Diagnostics;
    using Microsoft.CodeAnalysis.CSharp;

    public class DelegateCreationExpression : Call
    {
        internal void Parse(BoundDelegateCreationExpression boundDelegateCreationExpression)
        {
            base.Parse(boundDelegateCreationExpression);
            var argument = Deserialize(boundDelegateCreationExpression.Argument) as Expression;
            Debug.Assert(argument != null);

            if (boundDelegateCreationExpression.MethodOpt != null)
            {
                var methodGroup = new MethodGroup { ReceiverOpt = argument };
                methodGroup.Methods.Add(boundDelegateCreationExpression.MethodOpt);
                Arguments.Add(methodGroup);
            }
            else
            {
                Arguments.Add(argument);
            }
        }

        internal override void WriteTo(CCodeWriterBase c)
        {
            c.TextSpan("new");
            c.WhiteSpace();
            base.WriteTo(c);
        }
    }
}
