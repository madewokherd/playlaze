using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace playlaze
{
    public class MediaItem : PlaylistItem
    {
        public string PathOrUrl
        {
            get;
            // TODO: "set" requires a PlaylistAction for Undo history
        }

        public MediaItem(string path_or_url)
        {
            PathOrUrl = path_or_url;
        }

        public override string HumanReadableDescription()
        {
            return PathOrUrl;
        }

        public override DataObject GetDataObject()
        {
            var result = base.GetDataObject();
            if (File.Exists(PathOrUrl))
            {
                var files = new System.Collections.Specialized.StringCollection();
                files.Add(PathOrUrl);
                result.SetFileDropList(files);
            }
            result.SetText(PathOrUrl);
            return result;
        }
    }
}
