using System.Collections.Generic;
using Volo.Abp.Domain.Values;

namespace cima.Domain.Entities.Listings;

/// <summary>
/// Value Object que representa la ubicación de una propiedad.
/// Actualmente encapsula un string simple, pero permite futura expansión
/// a calle, número, código postal, coordenadas, etc.
/// </summary>
public class Address : ValueObject
{
    public string Value { get; private set; }

    private Address() 
    { 
        Value = default!;
    } // Para ORM

    public Address(string value)
    {
        Value = value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public static implicit operator string(Address address) => address.Value;
    public static implicit operator Address(string value) => new Address(value);
}
