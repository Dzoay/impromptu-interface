﻿// 
//  Copyright 2011  Ekon Benefits
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using ImpromptuInterface;
using ImpromptuInterface.Dynamic;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;
using BinderFlags = Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags;
using Info = Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo;
using InfoFlags = Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags;
using ImpromptuInterface.InvokeExt;
using ImpromptuInterface.Optimization;

#if SILVERLIGHT
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssertionException = Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException;
#elif !SELFRUNNER
using NUnit.Framework;
#endif

namespace UnitTestImpromptuInterface
{
    [TestFixture,TestClass]
    public class SingleMethodInvoke : Helper
    {
        [Test,TestMethod]
        public void TestDynamicSet()
        {
            dynamic tExpando = new ExpandoObject();

            var tSetValue = "1";

            Impromptu.InvokeSet(tExpando, "Test", tSetValue);

            Assert.AreEqual(tSetValue, tExpando.Test);

        }
		     [Test,TestMethod]
        public void TestPocoSet()
        {
            var tPoco = new PropPoco();

            var tSetValue = "1";

            Impromptu.InvokeSet(tPoco, "Prop1", tSetValue);

            Assert.AreEqual(tSetValue, tPoco.Prop1);

        }

             [Test, TestMethod]
             public void TestPocoSetNull()
             {
                 var tPoco = new PropPoco(){Prop1 = "Test default"};

                 String tSetValue = null;


                 Impromptu.InvokeSet(tPoco, "Prop1", tSetValue);

                 Assert.AreEqual(tSetValue, tPoco.Prop1);


             }
        
        [Test, TestMethod]
        public void TestConvert()
        {
            var tEl = new XElement("Test","45");

            var tCast = Impromptu.InvokeConvert(tEl, typeof (int), explict:true);
           
            Assert.AreEqual(typeof(int), tCast.GetType());
            Assert.AreEqual(45,tCast);
        }

        [Test, TestMethod]
        public void TestConstruct()
        {
            var tCast = Impromptu.InvokeConstuctor(typeof (List<object>), new object[]
                                                                              {
                                                                                  new string[] {"one", "two", "three"}
                                                                              });
        
            Assert.AreEqual("two", tCast[1]);
        }

        [Test, TestMethod]
        public void TestConstructOptional()
        {
            PocoOptConstructor tCast = Impromptu.InvokeConstuctor(typeof(PocoOptConstructor), "3".WithArgumentName("three"));

            Assert.AreEqual("-1", tCast.One);
            Assert.AreEqual("-2", tCast.Two);
            Assert.AreEqual("3", tCast.Three);
        }

        [Test, TestMethod]
        public void TestOptionalArgumentActivationNone()
            {
                AssertException<MissingMethodException>(() => Activator.CreateInstance<ImpromptuList>());

               var tList= Impromptu.InvokeConstuctor(typeof (ImpromptuList));


               Assert.AreEqual(typeof(ImpromptuList),tList.GetType());
          }


        [Test, TestMethod]
        public void TestConstructValueType()
        {
            var tCast = Impromptu.InvokeConstuctor(typeof(DateTime), 2009,1,20);

            Assert.AreEqual(20, tCast.Day);
        }
		
		     [Test, TestMethod]
        public void TestConstructValueTypeJustDynamic()
        {
			dynamic day =20;
			dynamic year =2009;
			dynamic month =1;
            var tCast = new DateTime(year,month,day);
			DateTime tDate = tCast;
            Assert.AreEqual(20, tDate.Day);
        }

        [Test, TestMethod]
        public void TestConstructprimativetype()
        {
            var tCast = Impromptu.InvokeConstuctor(typeof(Int32));

            Assert.AreEqual(default(Int32), tCast);
        }


        [Test, TestMethod]
        public void TestConstructDateTimeNoParams()
        {
            var tCast = Impromptu.InvokeConstuctor(typeof(DateTime));

            Assert.AreEqual(default(DateTime), tCast);
        }

        [Test, TestMethod]
        public void TestConstructOBjectNoParams()
        {
            var tCast = Impromptu.InvokeConstuctor(typeof(object));

            Assert.AreEqual(typeof(object), tCast.GetType());
        }

        [Test, TestMethod]
        public void TestConstructNullableprimativetype()
        {
            var tCast = Impromptu.InvokeConstuctor(typeof(Nullable<Int32>));

            Assert.AreEqual(null, tCast);
        }

        [Test, TestMethod]
        public void TestConstructGuid()
        {
            var tCast = Impromptu.InvokeConstuctor(typeof(Guid));

            Assert.AreEqual(default(Guid), tCast);
        }

        [Test, TestMethod]
        public void TestStaticCall()
        {
            dynamic i = 1;

            
            var tOut = Impromptu.InvokeMember(typeof (StaticType).WithStaticContext(),
                                              "Create".WithGenericArgs(typeof(bool)), 1);
            Assert.AreEqual(false,tOut);
        }

        [Test, TestMethod]
        public void TestImplicitConvert()
        {
            var tEl = 45;

            var tCast = Impromptu.InvokeConvert(tEl, typeof(long));

            Assert.AreEqual(typeof(long), tCast.GetType());
        }

        [Test, TestMethod]
        public void TestGetCached()
        {
            var tCached =new CacheableInvocation(InvocationKind.Get, "Prop1");

            var tSetValue = "1";
            var tAnon = new PropPoco{ Prop1 = tSetValue };

            var tOut = tCached.Invoke(tAnon);
            Assert.AreEqual(tSetValue, tOut);

            var tSetValue2 = "2";
            tAnon = new PropPoco { Prop1 = tSetValue2 };


            var tOut2 = tCached.Invoke(tAnon);


            Assert.AreEqual(tSetValue2, tOut2);

        }

        [Test, TestMethod]
        public void TestGetIndexer()
        {
       
            dynamic tSetValue = "1";
            var tAnon = new [] { tSetValue, "2" };


            string tOut = Impromptu.InvokeGetIndex(tAnon,0);

            Assert.AreEqual(tSetValue, tOut);

        }

        [Test, TestMethod]
        public void TestGetIndexerValue()
        {

            
            var tAnon = new int[] { 1, 2};


            int tOut = Impromptu.InvokeGetIndex(tAnon, 1);

            Assert.AreEqual(tAnon[1], tOut);

        }

        [Test, TestMethod]
        public void TestGetLengthArray()
        {
            var tAnon = new []  { "1", "2" };


            int tOut = Impromptu.InvokeGet(tAnon, "Length");

            Assert.AreEqual(2, tOut);

        }

        [Test, TestMethod]
        public void TestGetIndexerArray()
        {
			
			
            dynamic tSetValue = "1";
            var tAnon = new List<string> { tSetValue, "2" };
     

            string tOut = Impromptu.InvokeGetIndex(tAnon, 0);

            Assert.AreEqual(tSetValue, tOut);

        }

        [Test, TestMethod]
        public void TestSetIndexer()
        {
          
            dynamic tSetValue = "3";
            var tAnon =  new List<string> { "1", "2" };

            dynamic index = 0;
            dynamic tDyn = tAnon;


            Impromptu.InvokeSetIndex(tAnon, 0, tSetValue);

            Assert.AreEqual(tSetValue, tAnon[0]);

        }

     



        [Test, TestMethod]
        public void TestMethodDynamicPassAndGetValue()
        {
            dynamic tExpando = new ExpandoObject();
            tExpando.Func = new Func<int, string>(it => it.ToString());

            var tValue = 1;

            var tOut = Impromptu.InvokeMember(tExpando, "Func", tValue);

            Assert.AreEqual(tValue.ToString(), tOut);
        }


        [Test, TestMethod]
        public void TestMethodStaticOverloadingPassAndGetValue()
        {
            var tPoco = new OverloadingMethPoco();

            var tValue = 1;

            var tOut = Impromptu.InvokeMember(tPoco, "Func", tValue);

            Assert.AreEqual("int", tOut);

            Assert.AreEqual("int", (object)tOut); //should still be int because this uses runtime type


            var tOut2 = Impromptu.InvokeMember(tPoco, "Func", 1m);

            Assert.AreEqual("object", tOut2);

            var tOut3 = Impromptu.InvokeMember(tPoco, "Func", new{Anon =1});

            Assert.AreEqual("object", tOut3);
        }

        [Test, TestMethod]
        public void TestMethodStaticOverloadingPassAndGetValueArg()
        {
            var tPoco = new OverloadingMethPoco();

            var tValue = 1;

            var tOut = Impromptu.InvokeMember(tPoco, "Func", new InvokeArg("arg", tValue));

            Assert.AreEqual("int", tOut);

            Assert.AreEqual("int", (object)tOut); //should still be int because this uses runtime type


            var tOut2 = Impromptu.InvokeMember(tPoco, "Func", 1m);

            Assert.AreEqual("object", tOut2);

            var tOut3 = Impromptu.InvokeMember(tPoco, "Func", new { Anon = 1 });

            Assert.AreEqual("object", tOut3);
        }

        [Test, TestMethod]
        public void TestMethodStaticOverloadingPassAndGetValueArgOptional()
        {
            var tPoco = new OverloadingMethPoco();

            var tValue = 1;

            var arg = InvokeArg.Create;

            var tOut = Impromptu.InvokeMember(tPoco, "Func",  arg("two", tValue));

            Assert.AreEqual("object named", tOut);

            Assert.AreEqual("object named", (object)tOut); 
        }

        [Test, TestMethod]
        public void TestMethodStaticOverloadingPass2AndGetValueArgOptional()
        {
            var tPoco = new OverloadingMethPoco();

            var tValue = 1;

            var arg = InvokeArg.Create;

            var tOut = Impromptu.InvokeMember(tPoco, "Func", arg("two", tValue), arg("one", tValue));

            Assert.AreEqual("object named", tOut);

            Assert.AreEqual("object named", (object)tOut);
        }

         [Test, TestMethod]
        public void TestMethodStaticOverloadingPassAndGetValueNull()
        {
            var tPoco = new OverloadingMethPoco();

            var tValue = 1;

            var tOut = Impromptu.InvokeMember(tPoco, "Func", tValue);

            Assert.AreEqual("int", tOut);

            Assert.AreEqual("int", (object)tOut); //should still be int because this uses runtime type


            var tOut2 = Impromptu.InvokeMember(tPoco, "Func", 1m);

            Assert.AreEqual("object", tOut2);

            var tOut3 = Impromptu.InvokeMember(tPoco, "Func", null);

            Assert.AreEqual("object", tOut3);

            var tOut4 = Impromptu.InvokeMember(tPoco, "Func", null, null, "test", null, null, null);

            Assert.AreEqual("object 6", tOut4);

            var tOut5 = Impromptu.InvokeMember(tPoco, "Func", null, null, null, null, null, null);

            Assert.AreEqual("object 6", tOut5);
        }

        /// <summary>
        /// To dynamically invoke a method with out or ref parameters you need to know the signature
        /// </summary>
        [Test, TestMethod]
        public void TestOutMethod()
        {



            string tResult = String.Empty;

            var tPoco = new MethOutPoco();


            var tName = "Func";
            var tContext = GetType();
            var tBinder =
                Binder.InvokeMember(BinderFlags.None, tName, null, tContext,
                                            new[]
                                                {
                                                    Info.Create(
                                                        InfoFlags.None, null),
                                                    Info.Create(
                                                        InfoFlags.IsOut |
                                                        InfoFlags.UseCompileTimeType, null)
                                                });


            var tSite = Impromptu.CreateCallSite<DynamicTryString>(tBinder, tName, tContext);

          
            tSite.Target.Invoke(tSite, tPoco, out tResult);

            Assert.AreEqual("success", tResult);

        }


        [Test, TestMethod]
        public void TestMethodDynamicPassVoid()
        {
            var tTest = "Wrong";

            var tValue = "Correct";

            dynamic tExpando = new ExpandoObject();
            tExpando.Action = new Action<string>(it => tTest = it);



            Impromptu.InvokeMemberAction(tExpando, "Action", tValue);

            Assert.AreEqual(tValue, tTest);
        }


        [Test, TestMethod]
        public void TestMethodPocoGetValue()
        {
        

            var tValue = 1;

            var tOut = Impromptu.InvokeMember(tValue, "ToString");

            Assert.AreEqual(tValue.ToString(), tOut);
        }

  

        [Test, TestMethod]
        public void TestMethodPocoPassAndGetValue()
        {


            HelpTestPocoPassAndGetValue("Test", "Te");


            HelpTestPocoPassAndGetValue("Test", "st");
        }

        private void HelpTestPocoPassAndGetValue(string tValue, string tParam)
        {
            var tExpected = tValue.StartsWith(tParam);

            var tOut = Impromptu.InvokeMember(tValue, "StartsWith", tParam);

            Assert.AreEqual(tExpected, tOut);
        }


        [Test, TestMethod]
        public void TestGetDynamic()
        {

            var tSetValue = "1";
            dynamic tExpando = new ExpandoObject();
            tExpando.Test = tSetValue;



            var tOut = Impromptu.InvokeGet(tExpando, "Test");

            Assert.AreEqual(tSetValue, tOut);

        }
        [Test, TestMethod]
        public void TestStaticGet()
        {
            var tDate = Impromptu.InvokeGet(typeof(DateTime).WithStaticContext(), "Today");
            Assert.AreEqual(DateTime.Today, tDate);
        }

        [Test, TestMethod]
        public void TestStaticGet2()
        {
            var tVal = Impromptu.InvokeGet(typeof(StaticType).WithStaticContext(), "Test");
            Assert.AreEqual(true, tVal);
        }

        [Test, TestMethod]
        public void TestStaticSet()
        {
            int tValue = 12;
            Impromptu.InvokeSet(typeof(StaticType).WithStaticContext(), "TestSet", tValue);
            Assert.AreEqual(tValue, StaticType.TestSet);
        }

        [Test, TestMethod]
        public void TestStaticDateTimeMethod()
        {
            object tDateDyn = "01/20/2009";
            var tDate = Impromptu.InvokeMember(typeof(DateTime).WithStaticContext(), "Parse", tDateDyn);
            Assert.AreEqual(new DateTime(2009,1,20), tDate);
        }

        
        [Test, TestMethod]
        public void TestActionDynamicEvent()
        {
            dynamic tPoco = new PocoEvent();

            tPoco.Event += new EventHandler<EventArgs>((@object,args) => { });

        }

        [Test, TestMethod]
        public void TestIsEvent()
        {
            dynamic tPoco = new PocoEvent();

            var tResult = Impromptu.InvokeIsEvent(tPoco, "Event");

            Assert.AreEqual(true, tResult);
        }

         [Test, TestMethod]
        public void TestIsNotEvent()
        {
            dynamic tDynamic = new ImpromptuDictionary();

            tDynamic.Event = null;
        
            var tResult = Impromptu.InvokeIsEvent(tDynamic, "Event");

            Assert.AreEqual(false, tResult);

            bool tTest = false;
            bool tTest2 = false;


            tDynamic.Event += new EventHandler<EventArgs>((@object, args) => { tTest = true; });

            tDynamic.Event += new EventHandler<EventArgs>((@object, args) => { tTest2 = true; });
           
             Assert.AreEqual(false, tTest);

             Assert.AreEqual(false, tTest2);

             tDynamic.Event(null, null);

             Assert.AreEqual(true, tTest);

             Assert.AreEqual(true, tTest2);

        }

         [Test, TestMethod]
         public void TestPocoAddAssign()
         {
             var tPoco = new PocoEvent();
             bool tTest = false;

             Impromptu.InvokeAddAssign(tPoco, "Event", new EventHandler<EventArgs>((@object, args) => { tTest = true; }));

             tPoco.OnEvent(null, null);

             Assert.AreEqual(true, tTest);

             var tPoco2 = new PropPoco() { Prop2 = 3 };

             Impromptu.InvokeAddAssign(tPoco2, "Prop2", 4);

             Assert.AreEqual(7L, tPoco2.Prop2);
         }

         [Test, TestMethod]
         public void TestPocoSubtractAssign()
         {
             var tPoco = new PocoEvent();
             bool tTest = false;
             var tEvent = new EventHandler<EventArgs>((@object, args) => { tTest = true; });

             tPoco.Event += tEvent;

             Impromptu.InvokeSubtractAssign(tPoco, "Event", tEvent);

             tPoco.OnEvent(null, null);

             Assert.AreEqual(false, tTest);

             Impromptu.InvokeSubtractAssign(tPoco, "Event", tEvent);//Test Second Time

             var tPoco2 = new PropPoco() {Prop2 = 3};

             Impromptu.InvokeSubtractAssign(tPoco2, "Prop2", 4);

             Assert.AreEqual( -1L,tPoco2.Prop2);
         }

         [Test, TestMethod]
         public void TestDynamicAddAssign()
         {
             var tDyanmic = Build.NewObject(Prop2: 3, Event: null, OnEvent: new ThisAction<object, EventArgs>((@this, obj, args) => @this.Event(obj, args)));
             bool tTest = false;

             Impromptu.InvokeAddAssign(tDyanmic, "Event", new EventHandler<EventArgs>((@object, args) => { tTest = true; }));

             tDyanmic.OnEvent(null, null);

             Assert.AreEqual(true, tTest);

             Impromptu.InvokeAddAssign(tDyanmic, "Prop2", 4);

             Assert.AreEqual(7L, tDyanmic.Prop2);
         }

         [Test, TestMethod]
         public void TestDynamicSubtractAssign()
         {
             var tDyanmic = Build.NewObject(Prop2: 3, Event: null, OnEvent: new ThisAction<object, EventArgs>((@this, obj, args) => @this.Event(obj, args)));
             bool tTest = false;
             var tEvent = new EventHandler<EventArgs>((@object, args) => { tTest = true; });

             tDyanmic.Event += tEvent;

             Impromptu.InvokeSubtractAssign(tDyanmic, "Event", tEvent);

             tDyanmic.OnEvent(null, null);

             Assert.AreEqual(false, tTest);


             Impromptu.InvokeSubtractAssign(tDyanmic, "Prop2", 4);

             Assert.AreEqual(-1L, tDyanmic.Prop2);
         }

    }
    
}
