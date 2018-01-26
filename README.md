# NP.Roxy New Powerful Roxy IoC container and Code Generator

**Name Explanation**

The name of the package "Roxy" is a mix of two words: "Roslyn" and "Proxy" (even though I intend to make this package much more than a just a proxy generator).

**Why and What is Roxy**

The main purpose of Roxy is to introduce a better separation of concerns and correspondingly simplify the code.

Here are the tasks that the Roxy package addresses now and in the future.

Now:

* Converting an interface into a class using built-in conversion or easily created custom conversion mechanisms.
* Creating adaptors that adapt multiple classes to an interface or an abstract class or both.
* Achieving a greater separation of concerns by mixing behaviors with the objects that they modify.
* Easily accessing non-public properties and methods of 3rd party components.
* Creating smart mixins and allowing to easily swap implementations of the wrapped parts. This greatly enhances testing - e.g. you will be able to easily swap the real backend connection for a mock one.

In the future I plan to make Roxy a full blown IoC container. In particular it will allow:
* Resolving interfaces and abstract classes to concrete pre-specified (or generated) types.
* Producing singleton objects.
* Easily replacing interface or abstract class implementation with a different one.
Also, in the future I plan to remove some of the current Roxy limitations:
* Allowing to generate generic (not fully resolved) classes for generic interfaces.
* Allowing to deal with classes and interface that use method overloading.

I plan to publish a number of articles on the codeproject.com describing Roxy usage in much more detail. 
Here is the first article of the series [Introducing Roxy: Powerful Proxy Generation and IoC Container Package](https://www.codeproject.com/Articles/1227242/Introducing-Roxy-Powerful-Proxy-Generation-and)

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
