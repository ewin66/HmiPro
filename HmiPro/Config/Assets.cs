using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YCsharp.Util;

namespace HmiPro.Config {
    /// <summary>
    /// 程序使用到的图片、字典、配置Json静态文件路径
    /// <author>ychost</author>
    /// <date>2017-10-18</date>
    /// </summary>
    public class Assets {
        public Assets(string folder) {
            AssetsFolder = folder;
        }
        public readonly string AssetsFolder;

        //约定的文件夹
        public string ImagesFolder => AssetsFolder + @"\Images";
        public string DictsFoler => AssetsFolder + @"\Dicts";
        public string MocksFolder => AssetsFolder + @"\Mocks";
        public string BatsFoler => AssetsFolder + @"\Bats";


        //一些图标
        public string IconConfigAdmin => ImagesFolder + "\\config-admin.ico";
        public string IconJson => ImagesFolder + "\\json.ico";
        public string IconCsv => ImagesFolder + "\\csv.ico";
        public string IconAnalysis => ImagesFolder + "\\analysis.ico";
        public string IconMonitor => ImagesFolder + "\\monitor.png";
        public string IconConfigLoad => ImagesFolder + "\\config-load.ico";
        public string IconMessage => ImagesFolder + "\\message.ico";
        public string IconSetting => ImagesFolder + "\\setting.ico";
        public string IconKey => ImagesFolder + "\\key.ico";
        public string IconCpu => ImagesFolder + "\\cpu.ico";
        public string IconWrench => ImagesFolder + "\\wrench.ico";
        public string IconLicense => ImagesFolder + "\\license.ico";
        public string IconHelp => ImagesFolder + "\\help.ico";
        public string IconExit => ImagesFolder + "\\exit.ico";
        public string IconAbout => ImagesFolder + "\\about.ico";
        public string IconTask => ImagesFolder + "\\task.ico";
        public string IconAlert => ImagesFolder + "\\alert.ico";
        public string IconCall => ImagesFolder + "\\call.ico";
        public string IconQualityControl => ImagesFolder + "\\quality-control.ico";
        public string IconAnalysis2 => ImagesFolder + "\\analysis2.ico";
        public string IconCallUp => ImagesFolder + "\\call-up.ico";
        public string IconRepair => ImagesFolder + "\\repair.png";
        public string IconDataMonitor => ImagesFolder + "\\data-monitor.ico";
        public string IconAppBuild => ImagesFolder + "\\app-build.ico";
        public string IconAdd => ImagesFolder + "\\add.ico";
        public string IconDelete => ImagesFolder + "\\delete.ico";
        public string IconApp => ImagesFolder + "\\app.ico";
        public string IconImport => ImagesFolder + "\\import.ico";
        public string IconExport => ImagesFolder + "\\export.ico";
        public string IconConnect => ImagesFolder + "\\connect.ico";
        public string IconStart => ImagesFolder + "\\start.ico";
        public string IconStop => ImagesFolder + "\\stop.ico";
        public string IconSetting1 => ImagesFolder + "\\setting1.ico";
        public string IconSetting2 => ImagesFolder + "\\setting2.ico";
        public string IconSim => ImagesFolder + "\\sim.ico";
        public string IconStatsLines => ImagesFolder + "\\stats-lines.ico";
        public string IconTest => ImagesFolder + "\\test.ico";
        public string IconPrivacy => ImagesFolder + "\\privacy.ico";
        public string IconMonitor2 => ImagesFolder + "\\monitor2.ico";
        public string IconV => ImagesFolder + "\\v.ico";
        public string IconForklift => ImagesFolder + "\\forklift.ico";
        public string IconWorker => ImagesFolder + "\\worker.ico";

        //一些图片
        public string ImageBackground => ImagesFolder + "\\background.jpg";
        public string ImageIronMan => ImagesFolder + "\\iron-man.jpg";
        public string ImageSkull => ImagesFolder + "\\skull.jpg";

        //一些资源文件
        public string CraftsBomXls => DictsFoler + "\\工艺Bom.xls";
        public string MockMqSchTaskJson => MocksFolder + "\\MqSchTask.json";
        public string MockMqScanMaterial => MocksFolder + "\\MqScanMaterial.json";

        //一些可执行文件
        public string ExeType => Environment.Is64BitOperatingSystem ? "x64" : "x86";
        public string ExePath => AssetsFolder + "\\Exe\\";
        public string ExeNirCmd => ExePath + ExeType + "\\nircmd.exe";

        //一些脚本文件
        public string BatDeleteApp => BatsFoler + "\\delete-app.bat";
        public string BatStartApp => BatsFoler + "\\start-app.bat";
    }
}
