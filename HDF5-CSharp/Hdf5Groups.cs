using HDF.PInvoke;
using HDF5CSharp.DataTypes;
using System.Collections.Generic;
using System.Linq;

namespace HDF5CSharp;

public static partial class Hdf5
{
    public static bool GroupExists(long groupId, string groupName) => Hdf5Utils.ItemExists(groupId, groupName, Hdf5ElementType.Group);

    public static int CloseGroup(long groupId)
    {
        return H5G.close(groupId);
    }

    public static long CreateOrOpenGroup(long fileOrGroupId, string groupName)
    {
        string normalizedName = Hdf5Utils.NormalizedName(groupName);

        return (Hdf5Utils.ItemExists(fileOrGroupId, groupName, Hdf5ElementType.Group))
            ? H5G.open(fileOrGroupId, normalizedName)
            : H5G.create(fileOrGroupId, normalizedName);
    }




    /// <summary>
    /// creates a structure of groups at once.
    /// Close all groups except the last one as the user may use it.
    /// All upper groups in the hierarchy will be closed  
    /// </summary>
    /// <param name="groupOrFileId"></param>
    /// <param name="groupName"></param>
    /// <returns></returns>
    public static long CreateGroupRecursively(long groupOrFileId, string groupName) =>
        CreateGroupRecursively(groupOrFileId, groupName, true, false);


    /// <summary>
    /// creates a structure of groups at once
    /// </summary>
    /// <param name="groupOrFileId"></param>
    /// <param name="groupName"></param>
    /// <param name="closeAllGroups"> close the groups after before returns/</param>
    /// <param name="closeAlsoLastGroup">Also close the last returned group before return</param>
    /// <returns></returns>
    public static long CreateGroupRecursively(long groupOrFileId, string groupName, bool closeAllGroups, bool closeAlsoLastGroup)
    {
        IEnumerable<string> grps = groupName.Split('/');
        long gid = groupOrFileId;
        groupName = "";
        List<long> toplevelIds = new List<long>();
        foreach (var name in grps)
        {
            groupName = string.Concat(groupName, "/", name);
            gid = CreateOrOpenGroup(gid, groupName);
            toplevelIds.Add(gid);
        }

        if (closeAllGroups)
        {
            foreach (var id in toplevelIds)
            {
                if (id == gid) 
                {
                    if (!closeAlsoLastGroup)//don't close the returned one as it can be used by user
                    {
                        continue;
                    }
                }

                CloseGroup(id);
            }
        }

        return gid;
    }
    public static ulong NumberOfAttributes(int groupId, string groupName)
    {
        H5O.info_t info = new H5O.info_t();
        var gid = H5O.get_info(groupId, ref info);
        return info.num_attrs;
    }

    public static H5G.info_t GroupInfo(long groupId)
    {
        H5G.info_t info = new H5G.info_t();
        var gid = H5G.get_info(groupId, ref info);
        return info;
    }
}
