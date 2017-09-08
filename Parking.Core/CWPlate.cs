using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking.Data;
using Parking.Auxiliary;

namespace Parking.Core
{
    public class CWPlate
    {
        private PlateInfoManager manager = new PlateInfoManager();

        private static string configpath = null;

        public Response AddPlate(int warehouse, int hallID,string plateNum,string headimgpath,int trType)
        {
            Response resp = new Response();
            Log log = LogFactory.GetLogger("AddPlate");

            string virtualPath = "";
            try
            {
                if (!string.IsNullOrEmpty(headimgpath))
                {
                    string[] pathArr = headimgpath.Split('\\');
                    int count = pathArr.Length;
                    if (count > 2)
                    {
                        if (string.IsNullOrEmpty(configpath))
                        {
                            configpath= XMLHelper.GetRootNodeValueByXpath("root", "PlateVirtualPath");
                        }
                        virtualPath = configpath + pathArr[count - 2] + @"/" + pathArr[count - 1];
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }

            PlateMappingDev mapping = FindPlate(warehouse, hallID);
            log.Warn("开始写车牌信息！");
            if (mapping == null)
            {
                PlateMappingDev mp = new PlateMappingDev {
                    Warehouse=warehouse,
                    DeviceCode=hallID,
                    PlateNum=plateNum,
                    HeadImagePath= virtualPath,
                    PlateImagePath=null,
                    InDate=DateTime.Now
                };
                resp = manager.Add(mp);                
            }
            else
            {
                log.Warn("更新车牌信息！");
                if (string.IsNullOrEmpty(mapping.PlateNum))
                {
                    mapping.PlateNum = plateNum;
                    mapping.HeadImagePath = virtualPath;
                    mapping.InDate = DateTime.Now;

                    resp= manager.Update(mapping);                    
                }
                else
                {
                    if (mapping.PlateNum != plateNum)
                    {
                        //车牌识别不一致，则先判断时间，当前步进
                        Device hall = new CWDevice().Find(d=>d.Warehouse==warehouse&&d.DeviceCode==hallID);
                        if (hall != null)
                        {
                            if (hall.InStep == 30)
                            {
                                resp.Message = "30步时不接收任何的车牌识别";
                                return resp;
                            }
                        }
                        //在30秒内，有虚拟线圈触发，以当前的为准
                        if (DateTime.Compare(DateTime.Now, mapping.InDate.AddSeconds(30)) < 0 ||
                            DateTime.Compare(DateTime.Now, mapping.InDate.AddMinutes(5)) > 0)
                        {
                            //如果是虚拟线圈触发，则以当前为准
                            if (trType == 8)
                            {
                                mapping.PlateNum = plateNum;
                                mapping.HeadImagePath = virtualPath;
                                mapping.InDate = DateTime.Now;

                                resp = manager.Update(mapping);
                                if (resp.Code == 1)
                                {
                                    log.Info("更新车牌信息成功！");
                                }
                            }
                        }
                    }
                    else
                    {
                        //更新下页面
                        resp.Code = 1;
                    }
                   
                }
            }           
            //推送到页面中
            if (resp.Code == 1)
            {
                PlateDisplay disp = new PlateDisplay
                {
                    Warehouse = warehouse,
                    DeviceCode = hallID,
                    PlateNum = plateNum,
                    HeadImgVPath = virtualPath,
                    RcdDtime = DateTime.Now.ToString()
                };
                SingleCallback.Instance().WatchPlateInfo(disp);
            }
            return resp;
        }

        public Response UpdatePlate(PlateMappingDev mp)
        {
            Response resp = manager.Update(mp);
            return resp;
        }

        public PlateMappingDev FindPlate(int warehouse, int hallID)
        {
            return manager.Find(d => d.Warehouse == warehouse && d.DeviceCode == hallID);
        }

        public List<PlateMappingDev> FindList()
        {
            return manager.FindList();
        }
    }
}
