using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;

namespace DigitimateExample.UITests
{
    [TestFixture]
    public class Tests
    {
        AndroidApp app;

        [SetUp]
        public void BeforeEachTest()
        {
            app = ConfigureApp.Android.StartApp();
        }

        [Test]
        public void ClickingButtonTwiceShouldChangeItsLabel()
        {
            Func<AppQuery, AppQuery> validateButton = c => c.Button("validateButton");
            app.Tap(validateButton);

            Func<AppQuery, AppQuery> codeInput = c => c.TextField("codeInput");

            app.WaitForElement(codeInput);

            app.EnterText(codeInput, "000000");

            app.Tap(c => c.Button("button1"));

            AppResult[] result = app.Query(c => c.Button("validateButton").Text("Phone has been validated."));

            Assert.IsFalse(result.Any());
        }
    }
}

