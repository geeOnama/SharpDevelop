﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;

namespace ICSharpCode.NRefactory.CSharp.Resolver
{
	[TestFixture]
	public class InvocationTests : ResolverTestBase
	{
		// TODO: do we want to return the MemberResolveResult for the InvocationExpression, or only for it's target?
		
		[Test]
		public void MethodCallTest()
		{
			string program = @"class A {
	void Method() {
		$TargetMethod()$;
	}
	
	int TargetMethod() {
		return 3;
	}
}
";
			MemberResolveResult result = Resolve<MemberResolveResult>(program);
			Assert.AreEqual("A.TargetMethod", result.Member.FullName);
			Assert.AreEqual("System.Int32", result.Type.ReflectionName);
		}
		
		[Test]
		public void InvalidMethodCall()
		{
			string program = @"class A {
	void Method(string b) {
		$b.ThisMethodDoesNotExistOnString(b)$;
	}
}
";
			UnknownMethodResolveResult result = Resolve<UnknownMethodResolveResult>(program);
			Assert.AreEqual("ThisMethodDoesNotExistOnString", result.MemberName);
			Assert.AreEqual("System.String", result.TargetType.FullName);
			Assert.AreEqual(1, result.Parameters.Count);
			Assert.AreEqual("b", result.Parameters[0].Name);
			Assert.AreEqual("System.String", result.Parameters[0].Type.Resolve(context).ReflectionName);
			
			Assert.AreSame(SharedTypes.UnknownType, result.Type);
		}
		
		[Test, Ignore("Resolving type references is not implemented")]
		public void OverriddenMethodCall()
		{
			string program = @"class A {
	void Method() {
		$new B().GetRandomNumber()$;
	}
	
	public abstract int GetRandomNumber();
}
class B : A {
	public override int GetRandomNumber() {
		return 4; // chosen by fair dice roll.
		          // guaranteed to be random
	}
}
";
			MemberResolveResult result = Resolve<MemberResolveResult>(program);
			Assert.AreEqual("B.GetRandomNumber", result.Member.FullName);
		}
		
		[Test, Ignore("Resolving type references is not implemented")]
		public void OverriddenMethodCall2()
		{
			string program = @"class A {
	void Method() {
		$new B().GetRandomNumber(""x"", this)$;
	}
	
	public abstract int GetRandomNumber(string a, A b);
}
class B : A {
	public override int GetRandomNumber(string b, A a) {
		return 4; // chosen by fair dice roll.
		          // guaranteed to be random
	}
}
";
			MemberResolveResult result = Resolve<MemberResolveResult>(program);
			Assert.AreEqual("B.GetRandomNumber", result.Member.FullName);
		}
		
		[Test]
		public void ThisMethodCallTest()
		{
			string program = @"class A {
	void Method() {
		$this.TargetMethod()$;
	}
	
	int TargetMethod() {
		return 3;
	}
}
";
			MemberResolveResult result = Resolve<MemberResolveResult>(program);
			Assert.AreEqual("A.TargetMethod", result.Member.FullName);
			Assert.AreEqual("System.Int32", result.Type.ReflectionName);
		}
		
		[Test]
		public void VoidTest()
		{
			string program = @"using System;
class A {
	void TestMethod() {
		$TestMethod()$;
	}
}
";
			Assert.AreEqual("System.Void", Resolve(program).Type.ReflectionName);
		}
		
		[Test, Ignore("Type references cannot be resolved")]
		public void EventCallTest()
		{
			string program = @"using System;
class A {
	void Method() {
		$TestEvent(this, EventArgs.Empty)$;
	}
	
	public event EventHandler TestEvent;
}
";
			Assert.AreEqual("System.Void", Resolve(program).Type.ReflectionName);
		}
		
		/* TODO
		[Test]
		public void MethodGroupResolveTest()
		{
			string program = @"class A {
	void Method() {
		
	}
	
	void TargetMethod(int a) { }
	void TargetMethod<T>(T a) { }
}
";
			MethodGroupResolveResult result = Resolve<MethodGroupResolveResult>(program, "TargetMethod", 3);
			Assert.AreEqual("TargetMethod", result.Name);
			Assert.AreEqual(2, result.Methods.Count);
			
			result = Resolve<MethodGroupResolveResult>(program, "TargetMethod<string>", 3);
			Assert.AreEqual("TargetMethod", result.Name);
			Assert.AreEqual(1, result.Methods[0].Count);
			Assert.AreEqual("System.String", result.GetMethodIfSingleOverload().Parameters[0].ReturnType.FullyQualifiedName);
		}
		 */
	}
}
