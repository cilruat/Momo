using System;
using System.Collections.Generic;

using Android.Content;
using Android.Provider;
using Android.Widget;

using Momo.Droid;
using Xamarin.Forms;

using Plugin.CurrentActivity;

[assembly: Dependency(typeof(SaveContactService))]
namespace Momo.Droid
{
    public class SaveContactService : ISaveContactService
    {
        public void SaveContact(string name, string number)
        {
            var intent = new Intent(Intent.ActionInsert);
            intent.SetType(ContactsContract.Contacts.ContentType);
            intent.SetFlags(ActivityFlags.NewTask);
            intent.PutExtra(ContactsContract.Intents.Insert.Name, name);
            intent.PutExtra(ContactsContract.Intents.Insert.Phone, number);
            Android.App.Application.Context.StartActivity(intent);
        }

        public void SaveContact(List<KeyValuePair<string, string>> contacts)
        {
            for (int i = 0; i < contacts.Count; i++)
                WriteContact(contacts[i].Key, contacts[i].Value);
        }

        private void WriteContact(string name, string number)
        {
            List<ContentProviderOperation> ops = new List<ContentProviderOperation>();

            ContentProviderOperation.Builder builder = ContentProviderOperation.NewInsert(ContactsContract.RawContacts.ContentUri);
            builder.WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountType, null);
            builder.WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountName, null);
            ops.Add(builder.Build());

            // name
            builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
            builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0);
            builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.StructuredName.ContentItemType);
            builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.DisplayName, name);
            ops.Add(builder.Build());

            // number
            builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
            builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0);
            builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Phone.ContentItemType);
            builder.WithValue(ContactsContract.CommonDataKinds.Phone.Number, number);
            builder.WithValue(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Type, ContactsContract.CommonDataKinds.Phone.InterfaceConsts.TypeCustom);
            builder.WithValue(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Label, "Mobile");
            ops.Add(builder.Build());

            ContentProviderResult[] res;
            try
            {
                res = CrossCurrentActivity.Current.AppContext.ContentResolver.ApplyBatch(ContactsContract.Authority, ops);
                Toast.MakeText(CrossCurrentActivity.Current.AppContext, "저장 중입니다...", ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                Toast.MakeText(CrossCurrentActivity.Current.AppContext, "저장에 실패했습니다", ToastLength.Short).Show();
            }
        }
    }
}