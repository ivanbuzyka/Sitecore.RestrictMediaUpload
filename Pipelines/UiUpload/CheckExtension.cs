using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Pipelines.Upload;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace RestrictMediaUpload.Pipelines.UiUpload
{
  public class CheckExtension : UploadProcessor
  {
    public List<string> AllowedExtensions
    {
      get
      {
        var settingName = "RestrictMediaUpload.NonAdminUsers";
        if(Sitecore.Context.User.IsAdministrator)
        {
          settingName = "RestrictMediaUpload.AdminUsers";
        }

        var extensions = Sitecore.Configuration.Settings.GetSetting(settingName, String.Empty);

        return extensions.Split('|').Where(s => !string.IsNullOrEmpty(s)).ToList();
      }
    }
    public void Process(UploadArgs args)
    {
      Assert.ArgumentNotNull((object)args, nameof(args));

      foreach (string file in (NameObjectCollectionBase)args.Files)
      {
        HttpPostedFile fileObject = args.Files[file];

        if (!string.IsNullOrEmpty(fileObject.FileName))
        {
          // do the check only if any type configured to be allowed
          if(AllowedExtensions.Any())
          {
            if (!AllowedExtensions.Contains(fileObject.ContentType, StringComparer.InvariantCultureIgnoreCase))
            {
              string message = string.Format("The '{0}' file cannot be uploaded. The type '{1}' is not allowed", fileObject.FileName, fileObject.ContentType);
              Log.Warn(message, (object)typeof(CheckExtension));
              
              //the line below does not work due to some reason
              //args.UiResponseHandlerEx.FileCannotBeUploaded(StringUtil.EscapeJavascriptString(fileObject.FileName), "The file type is not allowed");              

              //this error message is shown when advanced upload dialog is used
              args.ErrorText = message;

              //following is the workaround to show popup with the error message
              HttpContext.Current.Response.Write(string.Format("<html><head><script type=\"text/JavaScript\" language=\"javascript\">alert('{0}');</script></head><body>Done</body></html>", message));
              args.AbortPipeline();
              break;
            }
          }
        }

        // The line below can be used to define whether it is zip archive and then all the files within zip can be checked
        //if (UploadProcessor.IsUnpack(args, file2))
        //{
        //  MemoryStream memoryStream = new MemoryStream();
        //  file2.InputStream.CopyTo((Stream)memoryStream);
        //  using (ZipArchive zipArchive = new ZipArchive((Stream)memoryStream))
        //  {
        //    try
        //    {
        //      foreach (ZipArchiveEntry entry in zipArchive.Entries)
        //      {
        //        if (!entry.FullName.EndsWith("/") && entry.Length > Settings.Media.MaxSizeInDatabase) 
        //        {
        //          string text = file2.FileName + "/" + entry.Name;
        //          args.UiResponseHandlerEx.FileTooBigForUpload(StringUtil.EscapeJavascriptString(text));
        //          args.ErrorText = string.Format("The file \"{0}\" is too big to be uploaded. The maximum size for uploading files is {1}.", (object)text, (object)MainUtil.FormatSize(Settings.Media.MaxSizeInDatabase));
        //          args.AbortPipeline();
        //          return;
        //        }
        //      }
        //    }
        //    catch (Exception ex)
        //    {
        //      args.ErrorText = ex.Message;
        //      args.AbortPipeline();
        //    }
        //  }
        //}
      }

    }
  }
}