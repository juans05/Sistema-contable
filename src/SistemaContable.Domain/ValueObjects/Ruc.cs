using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.ValueObjects
{
    public class Ruc
    {
        public string Value { get; private set; }

        private Ruc(string value)
        {
            Value = value;
        }

        public static Result<Ruc> Create(string ruc)
        {
            if (string.IsNullOrWhiteSpace(ruc))
                return Result<Ruc>.Failure("El RUC no puede estar vacío");

            if (ruc.Length != 11)
                return Result<Ruc>.Failure("El RUC debe tener 11 dígitos");

            if (!ruc.All(char.IsDigit))
                return Result<Ruc>.Failure("El RUC solo debe contener dígitos");

            return Result<Ruc>.Success(new Ruc(ruc));
        }

        public override string ToString() => Value;
    }
}
