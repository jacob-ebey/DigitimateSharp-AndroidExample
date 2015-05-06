
using System.Threading.Tasks;

using Android.App;
using Android.OS;
using Android.Telephony;
using Android.Widget;

using DigitimateSharp;
using Result = DigitimateSharp.Result;
using Android.Views;

namespace DigitimateExample
{
    [Activity(Label = "DigitimateExample", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        Digitimate validator;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get the UI elements we need to manipulate
            Button button = FindViewById<Button>(Resource.Id.validateButton);
            TextView error = FindViewById<TextView>(Resource.Id.error);

            // Create the validator. It is good to keep the strings in resources to translate.
            validator = new Digitimate("someemail@something.com", 6, Resources.GetString(Resource.String.send_message));

            // Subscribe to the button click
            button.Click += async (s, e) =>
            {
                // Get the number of the phone
                TelephonyManager manager = (TelephonyManager)GetSystemService(TelephonyService);

                if (manager == null || string.IsNullOrWhiteSpace(manager.Line1Number))
                {
                    error.SetText(Resource.String.error_no_number);
                    return;
                }

                string phoneNumber = manager.Line1Number;

                Task<Result> sendCodeTask = validator.SendCodeAsync(phoneNumber);

                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                View dialogView = LayoutInflater.Inflate(Resource.Layout.InputDialog, null);
                builder.SetTitle(Resource.String.dialog_title);
                EditText input = dialogView.FindViewById<EditText>(Resource.Id.codeInput);
                builder.SetView(dialogView);
                builder.SetPositiveButton("OK", async delegate(object sender, Android.Content.DialogClickEventArgs e2)
                    {
                        Result sendResult = await sendCodeTask;
                        if (!sendResult.OperationSuccessful)
                            return;

                        CheckCodeResult checkResult = await validator.CheckCodeAsync(phoneNumber, input.Text);

                        if (!checkResult.OperationSuccessful)
                        {
                            error.Text = string.Format(Resources.GetString(Resource.String.error_server), checkResult.ErrorMessage);
                        }
                        Application.SynchronizationContext.Post((e3) =>
                            {
                                if (checkResult.ValidCode)
                                    button.SetText(Resource.String.error_phone_validate_successful);
                                else
                                    button.SetText(Resource.String.error_phone_validate_failure);
                            }, null);
                    });
                AlertDialog dialog = builder.Show();

                await sendCodeTask.ContinueWith((sendTask) =>
                    {
                        Result sendResult = sendTask.Result;

                        if (sendResult.OperationSuccessful)
                            return;

                        error.Text = string.Format(Resources.GetString(Resource.String.error_server), sendResult.ErrorMessage);

                        dialog.Cancel();
                    });
            };
        }
    }
}


