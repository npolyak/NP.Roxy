# NP.Roxy
**Roxy IoC container and Code Generator**

I plan to publish a number of articles on the codeproject.com describing Roxy usage in detail. 
Below are a few samples previewing what one can do with Roxy:

**Creating a default implementation for an interface:**

    IPerson person = Core.Concretize<IPerson>();
    
    person.FirstName = "Joe";
    person.LastName = "Doe";
    person.Age = 35;
    person.Profession = "Astronaut";

    // test that the properties have indeed been assigned. 
    Console.WriteLine($"Name='{person.FirstName} {person.LastName}'; Age='{person.Age}'; Profession='{person.Profession}'");  


**Adapting a Class to an Interface including the Non-Public Members of the Class**

    IPerson person = 
        Core.CreateWrapperWithNonPublicMembers<IPerson, PersonImplementationWrapperInterface>("MyPersonImplementation"); 
        
    // set the properties
    person.FirstName = "Joe";
    person.LastName = "Doe";
    person.Age = 35;
    person.Profession = "Astronaut";
    
    // test that the wrapped properties and the method work
    Console.WriteLine($"Name/Profession='{person.GetFullNameAndProfession()}'; Age='{person.Age}'");  
    

**Adapting an Enumeration to an Interface using Static Extension (possibly Non-Public) Methods of the Enumeration**

    // we create an adaptor adapting ProductKind enumeration
    // to IProduct interface using extension methods from the static 
    // ProductKindExtensions class
    Core.CreateEnumerationAdapter<IProduct, ProductKind>(typeof(ProductKindExtensions));

    // enumeration value ProductKind.FinancialInstrument is converted into
    // IProduct interface
    IProduct product =
        Core.CreateEnumWrapper<IProduct, ProductKind>(ProductKind.FinancialInstrument);

    // we test the methods of the resulting object that implements IProduct interface.
    Console.WriteLine($"product: {product.GetDisplayName()}; Description: {product.GetDescription()}");
