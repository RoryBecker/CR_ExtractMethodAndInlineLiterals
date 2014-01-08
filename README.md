CR_ExtractMethodAndInlineLiterals
===========================

This project is a CodeRush plugin.

It automates, what I've found to be, a very common workflow.

Picture the code

    public void DoSomething()
    {
        SpecificMethod1(2);
        Class1.UtilMethod("Some Text");
        Class2.UtilMethod(64);
        Class3.UtilMethod("More Text");
        SpecificMethod2(256);
    }

You see that the series of calls to the Utility methods of Class1,Class2 and Class3 form a pattern which you want to reuse elsewhere.

If you extract these 3 lines normally, you'd carry the literal params with them into the extracted method thus:

    public void DoSomething()
    {
        SpecificMethod1(2);
        ExtractedMethod();
        SpecificMethod2(256);
    }
    private void ExtractedMethod()
    {
        Class1.UtilMethod("Some Text");
        Class2.UtilMethod(64);
        Class3.UtilMethod("More Text");
    }

...but that's not ideal, since you now have to find and promote each of those values to a parameter of the method.

Ideally we'd like something like...

    public void DoSomething()
    {
        SpecificMethod1(2);
        ExtractedMethod("Some Text", 64, "More Text");
        SpecificMethod2(256);
    }
    private void ExtractedMethod(string Param0, int Param1, string Param2)
    {
        Class1.UtilMethod(Param0);
        Class2.UtilMethod(Param1);
        Class3.UtilMethod(Param2);
    }

...which provides a nice new method ready to be reused from other locations.

You can use the prefious method (1x Extract + 4x Find and Promote params) or you could...

 - Perform "Introduce Local" refactoring on each of the params. (3 Refactorings)
 - Move the generated Local declarations above the target group of method calls. (3 Moves)
 - Extract the lines in question. (1 refactoring)
 - Perform an "Inline Temp" refactoring on each of the generated locals. (3 Refactorings)

But that's *10* operations. Even worse than the previous example (4 operations)

Don't worry. I've got your back :)

The new "Extract Method and Inline Literals" Refactoring automates all of that for you :)

So why not download, install and give it a whirl.
 
