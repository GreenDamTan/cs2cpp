﻿namespace Il2Native.Logic.DOM
{
    using System.CodeDom.Compiler;
    using Microsoft.CodeAnalysis;

    public class CCodeMethodDeclaration : CCodeDeclaration
    {
        public CCodeMethodDeclaration(IMethodSymbol method)
        {
            this.Method = method;
        }

        public IMethodSymbol Method { get; set; }

        public override void WriteTo(IndentedTextWriter itw, WriteSettings settings)
        {
            CCodeSerializer.WriteMethodDeclaration(itw, settings, this.Method, true);
            itw.WriteLine(";");
        }
    }
}