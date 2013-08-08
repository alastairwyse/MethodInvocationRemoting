MethodInvocationRemoting
------------------------

MethodInvocationRemoting is a framework which allows methods to be invoked remotely between C# and Java.

Method invocations are represented as objects and then serialized and transported from code in the calling language (e.g. Java), before being invoked on the receiving side (e.g. C#). Any return values from invoking the method are then passed back and handled in the calling language.

The object model is based around interfaces and an inversion of control / dependency injection design pattern, so users can easily substitute the provided classes which perform serialization and transport with their own classes.

MethodInvocationRemoting includes default transport classes which utilize Apache ActiveMQ message queuing, and serialization classes which convert objects to XML. The serialization classes provide built-in support for many native data types and single-dimension arrays of these types in C# and Java. Additionally the serialization classes can be easily extended to support serialization of user's custom data types.

Whilst remote method invocation is already available between C# and Java using protocols like web services, MethodInvocationRemoting provides bidirectionality, so that methods can be invoked from both the C# and Java sides.

Both the C# and Java projects include extensive unit tests to allow for easier extension and refactoring of the code.

For detailed information including explanation of the operation of the classes and sample applications, see http://www.oraclepermissiongenerator.net/methodinvocationremoting/