﻿namespace Il2Native.Logic.DOM2
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public class Conversion : Expression
    {
        public ITypeSymbol TypeSource { get; set; }

        public ITypeSymbol TypeDestination { get; set; }

        public Expression Operand { get; set; }

        public bool CCast { get; set; }

        private ConversionKind conversionKind;

        internal void Parse(BoundConversion boundConversion)
        {
            base.Parse(boundConversion);
            this.TypeSource = boundConversion.Operand.Type;
            this.TypeDestination = boundConversion.Type;
            this.Operand = Deserialize(boundConversion.Operand) as Expression;
            this.conversionKind = boundConversion.ConversionKind;
        }

        internal override void WriteTo(CCodeWriterBase c)
        {
            if (this.CCast)
            {
                c.WriteType(this.TypeDestination, true);
                c.TextSpan("(");
                this.Operand.WriteTo(c);
                c.TextSpan(")");
                return;
            }

            var interfaceCastRequired = this.conversionKind == ConversionKind.Boxing && this.TypeDestination.TypeKind == TypeKind.Interface;
            if (interfaceCastRequired)
            {
                c.TextSpan("interface_cast<");
                c.WriteType(this.TypeDestination, true);
                c.TextSpan(">");
                c.TextSpan("(");
            }

            if (this.WriteCast(c))
            {
                c.TextSpan("(");
                this.Operand.WriteTo(c);
                c.TextSpan(")");
            }

            if (interfaceCastRequired)
            {
                c.TextSpan(")");
            }
        }

        private bool WriteCast(CCodeWriterBase c)
        {
            switch (this.conversionKind)
            {
                case ConversionKind.MethodGroup:
                    var newDelegate = new DelegateCreationExpression { Type = this.TypeDestination };
                    newDelegate.Arguments.Add(this.Operand);
                    newDelegate.WriteTo(c);
                    return false;
                case ConversionKind.NullToPointer:
                    // The null pointer is represented as 0u.
                    c.TextSpan("nullptr");
                    return false;
                case ConversionKind.Boxing:
                    c.TextSpan("__box<");
                    c.WriteTypeFullName(this.TypeSource);
                    c.TextSpan(">");
                    break;
                case ConversionKind.Unboxing:
                    c.TextSpan("__unbox<");
                    c.WriteType(this.TypeDestination, true);
                    c.TextSpan(",");
                    c.WhiteSpace();
                    c.WriteTypeFullName(this.TypeDestination);
                    c.TextSpan(">");
                    break;
                case ConversionKind.ExplicitReference:
                case ConversionKind.ImplicitReference:

                    if (this.TypeDestination.TypeKind != TypeKind.TypeParameter &&
                        this.TypeSource.IsDerivedFrom(this.TypeDestination))
                    {
                        c.TextSpan("static_cast<");
                        c.WriteType(this.TypeDestination, true);
                        c.TextSpan(">");
                    }
                    else
                    {
                        if ((this.conversionKind == ConversionKind.ExplicitReference ||
                             this.conversionKind == ConversionKind.ImplicitReference)
                            && this.TypeDestination.TypeKind == TypeKind.Interface)
                        {
                            c.TextSpan("interface_cast<");
                        }
                        else
                        {
                            c.TextSpan("as<");
                        }

                        c.WriteType(this.TypeDestination, true);
                        c.TextSpan(">");
                    }

                    break;
                case ConversionKind.PointerToInteger:
                case ConversionKind.IntegerToPointer:
                case ConversionKind.PointerToPointer:
                    c.TextSpan("reinterpret_cast<");
                    c.WriteType(this.TypeDestination, true);
                    c.TextSpan(">");
                    break;
                case ConversionKind.Identity:
                    // for string
                    if (this.TypeSource.SpecialType == SpecialType.System_String &&
                        this.TypeDestination.TypeKind == TypeKind.PointerType)
                    {
                        c.TextSpan("&");
                        this.Operand.WriteTo(c);
                        c.TextSpan("->m_firstChar");
                        return false;
                    }

                    return true;
                default:
                    c.TextSpan("static_cast<");
                    c.WriteType(this.TypeDestination, true);
                    c.TextSpan(">");
                    break;
            }

            return true;
        }
    }
}
