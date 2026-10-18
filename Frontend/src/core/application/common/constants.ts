import { CgSignal } from 'react-icons/cg';
import { FiBookOpen, FiMonitor, FiUser, FiUsers } from 'react-icons/fi';
import { GrDocumentPerformance } from 'react-icons/gr';
import { IoSettingsOutline } from 'react-icons/io5';
import { MdOutlineMonitorHeart } from 'react-icons/md';
import { TfiWrite } from 'react-icons/tfi';
import { ToastPosition } from 'react-toastify';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import GroupedMenuItem from '@/core/application/common/models/left-menu/GroupedMenuItem';
import Menu from '@/core/application/common/models/left-menu/Menu';
import MenuItem from '@/core/application/common/models/left-menu/MenuItem';
import { MenuKeys } from '@/core/domain/enums/MenuKeys';

export default class Constants {
  //static API_BASE_URL = 'http://192.168.100.10:12704';s
  //static API_BASE_URL = 'https://localhost:12704';
  static API_TOKEN_STORAGE: string = 'AMR_ADMIN_API_TOKEN';
  static API_REFRESH_TOKEN_STORAGE: string = 'AMR_ADMIN_REFRESH_TOKEN';
  static API_ROLE_STORAGE: string = 'AMR_API_ROLE';

  static NODATA: string = 'No data';
  static DEBOUNCE_SEARCH: number = 800;
  static USER_IS_DISABLED = 'User+is+disabled';
  static LANGUAGE_SELECTED_STORAGE: string = 'LANGUAGE_SELECTED';
  static COLUMN_CUSTOMIZE_STORAGE: string = 'COLUMN_CUSTOMIZE_STORAGE';
  static LIST_DATA_TRANSLATE_STORAGE: string = 'LIST_DATA_TRANSLATE';
  static MAX_LEVEL: number = 3;
  static ReplaceRouterConfig = class {
    static type = ':type';
    static id = ':id';
  };

  static Role = class {
    static SuperAdmin = {
      key: 'SuperAdmin',
      value: 'SUPERADMIN',
    }; // full power in the application
    static Admin = {
      key: 'Quản lý',
      value: 'ADMIN',
    };
    static Staff = {
      key: 'Nhân viên',
      value: 'STAFF',
    };
    static Customer = {
      key: 'Khách hàng',
      value: 'CUSTOMER',
    };
  };

  static MonitoringPoints = class {
    static Connection = class {
      static Status = class {
        static Normal = {
          label: 'Bình thường',
          value: 'normal',
        };
        static LostConnection = {
          label: 'Mất kết nối',
          value: 'lost',
        };
      };
    };
  };

  static Reports = class {
    static TimeLabelUnit = class {
      static Daily = 'Thời gian (giờ)';
      static Monthly = 'Thời gian (ngày)';
      static Yearly = 'Thời gian (tháng)';
    };
    static DataUnit = class {
      static PKW = 'P(KW)';
      static KWH = 'kWh';
    };
    static TimeSelector = class {
      static General = class {
        static Daily = {
          label: 'Hàng ngày',
          value: 'Daily',
        };
        static Monthly = {
          label: 'Hàng tháng',
          value: 'Monthly',
        };
        static Yearly = {
          label: 'Hàng năm',
          value: 'Yearly',
        };
      };
      static LoadChart = [
        { label: 'Hàng ngày', value: 'Daily' },
        { label: 'Hàng tháng', value: 'Monthly' },
        { label: 'Hàng năm', value: 'Yearly' },
      ];
      static ElectricityConsumption = class {
        static TongSanLuongPTieuThu = [
          { label: 'Hàng ngày', value: 'Daily' },
          { label: 'Hàng tháng', value: 'Monthly' },
        ];
        static SanLuongPTieuThu = [{ label: 'Hàng tháng', value: 'Monthly' }];
        static TongSanLuongPChot = [{ label: 'Hàng năm', value: 'Yearly' }];
        static SanLuongPChot = [{ label: 'Hàng năm', value: 'Yearly' }];
        static TongSanLuongQChot = [{ label: 'Hàng năm', value: 'Yearly' }];
      };
      static OperatingParameter = [
        { label: 'Hàng ngày', value: 'Daily' },
        { label: 'Hàng tháng', value: 'Monthly' },
      ];
    };
    static Type = class {
      static LoadChart = class {
        static PhuTaiP = {
          label: 'Công suất tác dụng',
          value: 'CongSuatTacDung',
        };
        static PhuTaiQ = {
          label: 'Công suất phản kháng',
          value: 'CongSuatPhanKhang',
        };
      };
      static ElectricityConsumption = class {
        static TongSanLuongPTieuThu = {
          label: 'Sản lượng CSTD',
          fullLabel: 'Sản lượng công suất tác dụng',
          value: 'TongSanLuongPTieuThu',
        };
        static SanLuongPTieuThu = {
          label: 'Sản lượng CSTD (từng khung giờ)',
          fullLabel: 'Sản lượng công suất tác dụng (từng khung giờ)',
          value: 'SanLuongPTieuThu',
        };
        static TongSanLuongQChot = {
          label: 'Sản lượng CSPK chốt',
          fullLabel: 'Sản lượng công suất phản kháng chốt',
          value: 'TongSanLuongQChot',
        };
        static TongSanLuongPChot = {
          label: 'Sản lượng CSTD chốt',
          fullLabel: 'Sản lượng công suất tác dụng chốt',
          value: 'TongSanLuongPChot',
        };
        static SanLuongPChot = {
          label: 'Sản lượng CSTD chốt (từng khung giờ)',
          fullLabel: 'Sản lượng công suất tác dụng chốt (từng khung giờ)',
          value: 'SanLuongPChot',
        };
      };
      static OperatingParameter = class {
        static DienApTrungBinh3Pha = {
          label: 'Điện áp',
          value: 'DienAp',
        };
        static DongDienTrungBinh3Pha = {
          label: 'Dòng điện',
          value: 'DongDien',
        };
        static P = {
          label: 'Công suất tác dụng',
          value: 'CongSuatTacDung',
        };
        static Q = {
          label: 'Công suất phản kháng',
          value: 'CongSuatPhanKhang',
        };
      };
    };
  };

  static AccountStatus = class {
    static Active = 'ACTIVE';
    static Deactivate = 'DEACTIVATE';
    static Deleted = 'DELETED';
  };

  static GCSStatus = class {
    static Active = 'ACTIVE';
    static Deactivate = 'DEACTIVATE';
    static Deleted = 'DELETED';
  };
  static CustomerStatus = class {
    static Active = 'ACTIVE';
    static Deactivate = 'DEACTIVATE';
    static Deleted = 'DELETED';
  };

  static connectStatus = class {
    static Connected = 'CONNECTED';
    static Disconnected = 'DISCONNECTED';
  };

  static userStatus = class {
    static View = 'VIEW';
    static Edit = 'EDIT';
    static Add = 'ADD';
  };

  static FileType = class {
    static List = [
      'png',
      'jpeg',
      'jpg',
      'pdf',
      'xlsx',
      'xls',
      'doc',
      'docx',
      'ppt',
      'pptx',
      'zip',
      'rar',
    ];
  };
  static Chart = class {
    static ChartOptions = class {
      static FontSize = 16;
    };
  };

  static MenuConfigs = class {
    static DashboardMenu = 'dashboard';
    static MainMenu = new Menu('light', 'main-menu', [MenuKeys.MainMenu], 'inline', [
      new GroupedMenuItem(
        [],
        MenuKeys.MonitoringPoints,
        FiMonitor,
        'Giám sát điểm đo',
        'Giám sát điểm đo',
        [Constants.Role.Admin.value, Constants.Role.SuperAdmin.value, Constants.Role.Staff.value],
        [
          new MenuItem(
            [],
            MenuKeys.MonitoringPointsList,
            null,
            'Thông số tổng hợp',
            'Thông số tổng hợp',
            [
              Constants.Role.Admin.value,
              Constants.Role.SuperAdmin.value,
              Constants.Role.Staff.value,
            ],
            ROUTE_PATH.POINT_MONITORING,
            MenuKeys.MonitoringPoints
          ),
          new MenuItem(
            [],
            MenuKeys.MonitoringPointsEvent,
            null,
            'Sự kiện',
            'Sự kiện',
            [
              Constants.Role.Admin.value,
              Constants.Role.SuperAdmin.value,
              Constants.Role.Staff.value,
            ],
            ROUTE_PATH.POINT_MONITORING_EVENT,
            MenuKeys.MonitoringPoints
          ),
          new MenuItem(
            [],
            MenuKeys.MonitoringPointsWarning,
            null,
            'Cảnh báo',
            'Cảnh báo',
            [
              Constants.Role.Admin.value,
              Constants.Role.SuperAdmin.value,
              Constants.Role.Staff.value,
            ],
            ROUTE_PATH.POINT_MONITORING_WARNING,
            MenuKeys.MonitoringPoints
          ),
        ]
      ),
      new MenuItem(
        [],
        MenuKeys.GCSsum,
        TfiWrite,
        'Ghi chỉ số',
        'Ghi chỉ số',
        [Constants.Role.Admin.value, Constants.Role.SuperAdmin.value, Constants.Role.Staff.value],
        ROUTE_PATH.GCS_SUM,
        null
      ),
      new GroupedMenuItem(
        [],
        MenuKeys.Report,
        CgSignal,
        'Báo cáo',
        'Báo cáo',
        [
          Constants.Role.Admin.value,
          Constants.Role.SuperAdmin.value,
          Constants.Role.Staff.value,
          Constants.Role.Customer.value,
        ],
        [
          new MenuItem(
            [],
            MenuKeys.OperatingParameter,
            null,
            'Thông số vận hành',
            'Thông số vận hành',
            [
              Constants.Role.Admin.value,
              Constants.Role.SuperAdmin.value,
              Constants.Role.Staff.value,
              Constants.Role.Customer.value,
            ],
            ROUTE_PATH.OPERATING_PARAMETER,
            MenuKeys.Report
          ),
          new MenuItem(
            [],
            MenuKeys.HalfHourFinalizedEnergyReport,
            null,
            'Chỉ số điện',
            'Chỉ số điện',
            [
              Constants.Role.Admin.value,
              Constants.Role.SuperAdmin.value,
              Constants.Role.Staff.value,
              Constants.Role.Customer.value,
            ],
            ROUTE_PATH.FINALIZED_ENERGY_REPORT,
            MenuKeys.Report
          ),
          new MenuItem(
            [],
            MenuKeys.ElectricityConsumption,
            null,
            'Sản lượng điện',
            'Sản lượng điện',
            [
              Constants.Role.Admin.value,
              Constants.Role.SuperAdmin.value,
              Constants.Role.Staff.value,
              Constants.Role.Customer.value,
            ],
            ROUTE_PATH.ELECTRICITY_CONSUMPTION,
            MenuKeys.Report
          ),
          new MenuItem(
            [],
            MenuKeys.LoadCurve,
            null,
            'Đồ thị phụ tải',
            'Đồ thị phụ tải',
            [
              Constants.Role.Admin.value,
              Constants.Role.SuperAdmin.value,
              Constants.Role.Staff.value,
              Constants.Role.Customer.value,
            ],
            ROUTE_PATH.LOAD_CURVE,
            MenuKeys.Report
          ),
          new MenuItem(
            [],
            MenuKeys.TotalOutputMeasurementPoints,
            null,
            'Tổng sản lượng các điểm đo',
            'Tổng sản lượng các điểm đo',
            [
              Constants.Role.Admin.value,
              Constants.Role.SuperAdmin.value,
              Constants.Role.Staff.value,
              Constants.Role.Customer.value,
            ],
            ROUTE_PATH.TOTAL_OUTPUT_MEASUREMENT_POINTS,
            MenuKeys.Report
          ),
        ]
      ),

      new MenuItem(
        [],
        MenuKeys.Clients,
        FiUsers,
        'Quản lý khách hàng',
        'Quản lý khách hàng',
        [Constants.Role.Admin.value, Constants.Role.SuperAdmin.value, Constants.Role.Staff.value],
        ROUTE_PATH.CUSTOMER_MANAGEMENT,
        null
      ),
      new MenuItem(
        [],
        MenuKeys.ManagePoints,
        MdOutlineMonitorHeart,
        'Quản lý điểm đo',
        'Quản lý điểm đo',
        [Constants.Role.Admin.value, Constants.Role.SuperAdmin.value, Constants.Role.Staff.value],
        ROUTE_PATH.POINT_MANAGEMENT,
        null
      ),
      new MenuItem(
        [],
        MenuKeys.ManageGCS,
        FiBookOpen,
        'Quản lý sổ GCS',
        'Quản lý sổ GCS',
        [Constants.Role.Admin.value, Constants.Role.SuperAdmin.value, Constants.Role.Staff.value],
        ROUTE_PATH.GCS_MANAGEMENT,
        null
      ),
      // new MenuItem(
      //   [],
      //   MenuKeys.ManageReports,
      //   IoDocumentTextOutline,
      //   'Quản lý báo cáo',
      //   'Quản lý báo cáo',
      //   [RoleName.SUPERADMIN,RoleName.ADMIN,RoleName.STAFF],
      //   ROUTE_PATH.REPORT_MANAGEMENT,
      //   null
      // ),
      new MenuItem(
        [],
        MenuKeys.noticeManagement,
        GrDocumentPerformance,
        'Quản lý phiếu báo',
        'Quản lý phiếu báo',
        [Constants.Role.Admin.value, Constants.Role.SuperAdmin.value, Constants.Role.Staff.value],
        ROUTE_PATH.NOTICE_MANAGEMENT,
        null
      ),
      new MenuItem(
        [],
        MenuKeys.Accounts,
        FiUser,
        'Quản lý tài khoản',
        'Quản lý tài khoản',
        [Constants.Role.SuperAdmin.value],
        ROUTE_PATH.ACCOUNT_MANAGEMENT,
        null
      ),
      new GroupedMenuItem(
        [],
        MenuKeys.ConfigSysTem,
        IoSettingsOutline,
        'Cài đặt hệ thống',
        'Cài đặt hệ thống',
        [Constants.Role.SuperAdmin.value],
        [
          new MenuItem(
            [],
            MenuKeys.Config,
            null,
            'Dữ liệu điểm đo',
            'Dữ liệu điểm đo',
            [Constants.Role.SuperAdmin.value],
            ROUTE_PATH.CONFIGURATION,
            MenuKeys.MonitoringPoints
          ),
          new MenuItem(
            [],
            MenuKeys.Contact,
            null,
            'Thông tin liên hệ',
            'Thông tin liên hệ',
            [Constants.Role.SuperAdmin.value],
            ROUTE_PATH.CONTACT,
            MenuKeys.MonitoringPoints
          ),
          new MenuItem(
            [],
            MenuKeys.authLock,
            null,
            'Khóa đăng nhập',
            'Khóa đăng nhập',
            [Constants.Role.SuperAdmin.value],
            ROUTE_PATH.AUTH_LOCK,
            MenuKeys.MonitoringPoints
          ),
        ]
      ),
    ]);
  };
  static Logger = class {
    static DateTimeFormat = 'yyyy-MM-DD HH:mm:ss';
    static DateFormat = 'yyyy-MM-DD';
  };
  static DateTime = class {
    static DateTimeFormat = 'DD/MM/YYYY HH:mm:ss';
    static DateFormat = 'DD/MM/YYYY';
    static MonthYearFormat = 'MM/YYYY';
    static YearFormat = 'YYYY';
  };

  static LocalStorage = class {
    static LeftMenu: string = 'LEFT_MENU';
    static ApiToken: string = 'AMR_ADMIN_API_TOKEN';
  };

  static SessionStorage = class {
    static LeftMenu: string = 'LEFT_MENU';
  };

  static OAuthProvider = class {
    static Google: number = 1;
    static Facebook: number = 2;
    static Apple: number = 3;
    static Microsoft: number = 4;
  };

  static socketIoEndpoint = class {
    static summary: string = 'listnotificationhub';
    static event: string = 'notificationeventhub';
    static dateTime: string = 'detailnotificationdatetimehub';
    static detail: string = 'detailnotificationhub';
  };

  static socketIoStatus = class {
    static CountAlert: string = 'CountAlert';
    static ReceiveEvent: string = 'ReceiveEvent';
    static ReceiveAlert: string = 'ReceiveAlert';
    static ReceiveUpdate: string = 'ReceiveUpdate';
  };

  static ToastMessage = class {
    static Notification = class {
      static Position: ToastPosition = 'top-right';
      static Duration: number = 6000;
    };
    static Confirmation = class {};
  };

  static PaginationConfigs = class {
    static Size: number = 30;
    static SizeSearch: number = 20;
    static SizeSearchPage: number = 8;
    static LimitSize: number = 50;
    static MaxSize: number = 100;
    static AllSize: number = 9000;
    static LargeSize: number = 300;

    static PageSizeList: Array<any> = [
      { label: '30', value: 30 },
      { label: '50', value: 50 },
      { label: '70', value: 70 },
      { label: '100', value: 100 },
    ];
    static PageSizeListSearchJira: Array<any> = [
      { label: '30', value: 30 },
      { label: '50', value: 50 },
    ];
  };

  static LockAuth = class {
    static LOCKLOGIN = 'LOCKLOGIN';
    static NUMBEROFINCORRECTENTRIES = 'NUMBEROFINCORRECTENTRIES';
    static SYSTEMADMINISTRATIONFULLNAME = 'SYSTEMADMINISTRATIONFULLNAME';
    static SYSTEMADMINISTRATIONEMAIL = 'SYSTEMADMINISTRATIONEMAIL';
    static SYSTEMADMINISTRATIONPHONE = 'SYSTEMADMINISTRATIONPHONE';
    static CUSTOMERCAREFULLNAME = 'CUSTOMERCAREFULLNAME';
    static CUSTOMERCAREEMAIL = 'CUSTOMERCAREEMAIL';
    static CUSTOMERCAREPHONE = 'CUSTOMERCAREPHONE';
  };

  // Tag Name list
  static DetailOperatingParams: Array<any> = [
    {
      label_1: 'Dòng điện',
      value: [
        {
          attribute: 'dongDienPhaA',
          unit: 'dongDienPhaAUnit',
          symbol: 'Ia',
          quanlity: 'dongDienPhaAQuanlity',
          timestamp: 'dongDienPhaACreatedTime',
        },
        {
          attribute: 'dongDienPhaB',
          unit: 'dongDienPhaBUnit',
          symbol: 'Ib',
          quanlity: 'dongDienPhaBQuanlity',
          timestamp: 'dongDienPhaBCreatedTime',
        },
        {
          attribute: 'dongDienPhaC',
          unit: 'dongDienPhaCUnit',
          symbol: 'Ic',
          quanlity: 'dongDienPhaCQuanlity',
          timestamp: 'dongDienPhaCCreatedTime',
        },
      ],
    },
    {
      label_1: 'Điện áp',
      value: [
        {
          attribute: 'dienApPhaA',
          unit: 'dienApPhaAUnit',
          symbol: 'Ua',
          quanlity: 'dienApPhaAQuanlity',
          timestamp: 'dienApPhaACreatedTime',
        },
        {
          attribute: 'dienApPhaB',
          unit: 'dienApPhaBUnit',
          symbol: 'Ub',
          quanlity: 'dienApPhaBQuanlity',
          timestamp: 'dienApPhaBCreatedTime',
        },
        {
          attribute: 'dienApPhaC',
          unit: 'dienApPhaCUnit',
          symbol: 'Uc',
          quanlity: 'dienApPhaCQuanlity',
          timestamp: 'dienApPhaCCreatedTime',
        },
      ],
    },
    {
      label_1: 'Tần số',
      value: [
        {
          attribute: 'tanSoHeThong',
          unit: 'tanSoHeThongUnit',
          symbol: 'F',
          quanlity: 'tanSoHeThongQuanlity',
          timestamp: 'tanSoHeThongCreatedTime',
        },
      ],
    },
    {
      label_1: 'Công suất tác dụng',
      value: [
        {
          attribute: 'congSuatTacDungPhaA',
          unit: 'congSuatTacDungPhaAUnit',
          symbol: 'Pa',
          quanlity: 'congSuatTacDungPhaAQuanlity',
          timestamp: 'congSuatTacDungPhaACreatedTime',
        },
        {
          attribute: 'congSuatTacDungPhaB',
          unit: 'congSuatTacDungPhaBUnit',
          symbol: 'Pb',
          quanlity: 'congSuatTacDungPhaBQuanlity',
          timestamp: 'congSuatTacDungPhaBCreatedTime',
        },
        {
          attribute: 'congSuatTacDungPhaC',
          unit: 'congSuatTacDungPhaCUnit',
          symbol: 'Pc',
          quanlity: 'congSuatTacDungPhaCQuanlity',
          timestamp: 'congSuatTacDungPhaCCreatedTime',
        },
        {
          attribute: 'congSuatTacDungTong3Pha',
          unit: 'congSuatTacDungTong3PhaUnit',
          symbol: 'Pt',
          quanlity: 'congSuatTacDungTong3PhaQuanlity',
          timestamp: 'congSuatTacDungTong3PhaCreatedTime',
        },
      ],
    },
    {
      label_1: 'Công suất phản kháng',
      value: [
        {
          attribute: 'congSuatPhanKhangPhaA',
          unit: 'congSuatPhanKhangPhaAUnit',
          symbol: 'Qa',
          quanlity: 'congSuatPhanKhangPhaAQuanlity',
          timestamp: 'congSuatPhanKhangPhaACreatedTime',
        },
        {
          attribute: 'congSuatPhanKhangPhaB',
          unit: 'congSuatPhanKhangPhaBUnit',
          symbol: 'Qb',
          quanlity: 'congSuatPhanKhangPhaBQuanlity',
          timestamp: 'congSuatPhanKhangPhaBCreatedTime',
        },
        {
          attribute: 'congSuatPhanKhangPhaC',
          unit: 'congSuatPhanKhangPhaCUnit',
          symbol: 'Qc',
          quanlity: 'congSuatPhanKhangPhaCQuanlity',
          timestamp: 'congSuatPhanKhangPhaCCreatedTime',
        },
        {
          attribute: 'congSuatPhanKhangTong3Pha',
          unit: 'congSuatPhanKhangTong3PhaUnit',
          symbol: 'Qt',
          quanlity: 'congSuatPhanKhangTong3PhaQuanlity',
          timestamp: 'congSuatPhanKhangTong3PhaCreatedTime',
        },
      ],
    },
    {
      label_1: 'Hệ số công suất',
      value: [
        {
          attribute: 'heSoCongSuatPFPhaA',
          unit: '',
          symbol: 'PFa',
          quanlity: 'heSoCongSuatPFPhaAQuanlity',
          timestamp: 'heSoCongSuatPFPhaACreatedTime',
        },
        {
          attribute: 'heSoCongSuatPFPhaB',
          unit: '',
          symbol: 'PFb',
          quanlity: 'heSoCongSuatPFPhaBQuanlity',
          timestamp: 'heSoCongSuatPFPhaBCreatedTime',
        },
        {
          attribute: 'heSoCongSuatPFPhaC',
          unit: '',
          symbol: 'PFc',
          quanlity: 'heSoCongSuatPFPhaCQuanlity',
          timestamp: 'heSoCongSuatPFPhaCCreatedTime',
        },
        {
          attribute: 'heSoCongSuatPFTrungBinh3Pha',
          unit: '',
          symbol: 'PF',
          quanlity: 'heSoCongSuatPFTrungBinh3PhaQuanlity',
          timestamp: 'heSoCongSuatPFTrungBinh3PhaCreatedTime',
        },
      ],
    },
  ];

  static DetailElectricParams: Array<any> = [
    {
      // Chỉ số tích lũy
      label_1: 'Chiều giao',
      value: [
        {
          attribute: 'chiSoTichLuyDienNangTacDungAPBieuGioBT',
          unit: 'chiSoTichLuyDienNangTacDungAPBieuGioBTUnit',
          symbol: 'AP BT',
          quanlity: 'chiSoTichLuyDienNangTacDungAPBieuGioBTQuanlity',
          timestamp: 'chiSoTichLuyDienNangTacDungAPBieuGioBTCreatedTime',
        },
        {
          attribute: 'chiSoTichLuyDienNangTacDungAPBieuGioCD',
          unit: 'chiSoTichLuyDienNangTacDungAPBieuGioCDUnit',
          symbol: 'AP CD',
          quanlity: 'chiSoTichLuyDienNangTacDungAPBieuGioCDQuanlity',
          timestamp: 'chiSoTichLuyDienNangTacDungAPBieuGioCDCreatedTime',
        },
        {
          attribute: 'chiSoTichLuyDienNangTacDungAPBieuGioTD',
          unit: 'chiSoTichLuyDienNangTacDungAPBieuGioTDUnit',
          symbol: 'AP TD',
          quanlity: 'chiSoTichLuyDienNangTacDungAPBieuGioTDQuanlity',
          timestamp: 'chiSoTichLuyDienNangTacDungAPBieuGioTDCreatedTime',
        },
        {
          attribute: 'chiSoTichLuyDienNangTacDungAPTong',
          unit: 'chiSoTichLuyDienNangTacDungAPTongUnit',
          symbol: 'AP tổng',
          quanlity: 'chiSoTichLuyDienNangTacDungAPTongQuanlity',
          timestamp: 'chiSoTichLuyDienNangTacDungAPTongCreatedTime',
        },
        {
          attribute: 'chiSoTichLuyDienNangPhanKhangAQTong',
          unit: 'chiSoTichLuyDienNangPhanKhangAQTongUnit',
          symbol: 'AQ tổng',
          quanlity: 'chiSoTichLuyDienNangPhanKhangAQTongQuanlity',
          timestamp: 'chiSoTichLuyDienNangPhanKhangAQTongCreatedTime',
        },
      ],
    },
    {
      label_1: 'Chiều nhận',
      value: [
        {
          attribute: 'chiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan',
          unit: 'chiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUnit',
          symbol: 'AP BT',
          quanlity: 'chiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanQuanlity',
          timestamp: 'chiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanCreatedTime',
        },
        {
          attribute: 'chiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan',
          unit: 'chiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUnit',
          symbol: 'AP CD',
          quanlity: 'chiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanQuanlity',
          timestamp: 'chiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanCreatedTime',
        },
        {
          attribute: 'chiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan',
          unit: 'chiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUnit',
          symbol: 'AP TD',
          quanlity: 'chiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanQuanlity',
          timestamp: 'chiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanCreatedTime',
        },
        {
          attribute: 'chiSoTichLuyDienNangTacDungAPTongChieuNhan',
          unit: 'chiSoTichLuyDienNangTacDungAPTongChieuNhanUnit',
          symbol: 'AP tổng',
          quanlity: 'chiSoTichLuyDienNangTacDungAPTongChieuNhanQuanlity',
          timestamp: 'chiSoTichLuyDienNangTacDungAPTongChieuNhanCreatedTime',
        },
        {
          attribute: 'chiSoTichLuyDienNangPhanKhangAQTongChieuNhan',
          unit: 'chiSoTichLuyDienNangPhanKhangAQTongChieuNhanUnit',
          symbol: 'AQ tổng',
          quanlity: 'chiSoTichLuyDienNangPhanKhangAQTongChieuNhanQuanlity',
          timestamp: 'chiSoTichLuyDienNangPhanKhangAQTongChieuNhanCreatedTime',
        },
      ],
    },

    // Chỉ số chốt
    {
      label_1: 'Chiều giao',
      value: [
        {
          attribute: 'chiSoChotDienNangAPGioBT',
          unit: 'chiSoChotDienNangAPGioBTUnit',
          symbol: 'AP BT',
          quanlity: 'chiSoChotDienNangAPGioBTQuanlity',
          timestamp: 'chiSoChotDienNangAPGioBTCreatedTime',
        },
        {
          attribute: 'chiSoChotDienNangAPGioCD',
          unit: 'chiSoChotDienNangAPGioCDUnit',
          symbol: 'AP CD',
          quanlity: 'chiSoChotDienNangAPGioCDQuanlity',
          timestamp: 'chiSoChotDienNangAPGioCDCreatedTime',
        },
        {
          attribute: 'chiSoChotDienNangAPGioTD',
          unit: 'chiSoChotDienNangAPGioTDUnit',
          symbol: 'AP TD',
          quanlity: 'chiSoChotDienNangAPGioTDQuanlity',
          timestamp: 'chiSoChotDienNangAPGioTDCreatedTime',
        },
        {
          attribute: 'chiSoChotDienNangAPTong',
          unit: 'chiSoChotDienNangAPTongUnit',
          symbol: 'AP tổng',
          quanlity: 'chiSoChotDienNangAPTongQuanlity',
          timestamp: 'chiSoChotDienNangAPTongCreatedTime',
        },
        {
          attribute: 'chiSoChotDienNangAQTong',
          unit: 'chiSoChotDienNangAQTongUnit',
          symbol: 'AQ tổng',
          quanlity: 'chiSoChotDienNangAQTongQuanlity',
          timestamp: 'chiSoChotDienNangAQTongCreatedTime',
        },
      ],
    },
    {
      label_1: 'Chiều nhận',
      value: [
        {
          attribute: 'chiSoChotDienNangAPGioBTChieuNhan',
          unit: 'chiSoChotDienNangAPGioBTChieuNhanUnit',
          symbol: 'AP BT',
          quanlity: 'chiSoChotDienNangAPGioBTChieuNhanQuanlity',
          timestamp: 'chiSoChotDienNangAPGioBTChieuNhanCreatedTime',
        },
        {
          attribute: 'chiSoChotDienNangAPGioCDChieuNhan',
          unit: 'chiSoChotDienNangAPGioCDChieuNhanUnit',
          symbol: 'AP CD',
          quanlity: 'chiSoChotDienNangAPGioCDChieuNhanQuanlity',
          timestamp: 'chiSoChotDienNangAPGioCDChieuNhanCreatedTime',
        },
        {
          attribute: 'chiSoChotDienNangAPGioTDChieuNhan',
          unit: 'chiSoChotDienNangAPGioTDChieuNhanUnit',
          symbol: 'AP TD',
          quanlity: 'chiSoChotDienNangAPGioTDChieuNhanQuanlity',
          timestamp: 'chiSoChotDienNangAPGioTDChieuNhanCreatedTime',
        },
        {
          attribute: 'chiSoChotDienNangAPTongChieuNhan',
          unit: 'chiSoChotDienNangAPTongChieuNhanUnit',
          symbol: 'AP tổng',
          quanlity: 'chiSoChotDienNangAPTongChieuNhanQuanlity',
          timestamp: 'chiSoChotDienNangAPTongChieuNhanCreatedTime',
        },
        {
          attribute: 'chiSoChotDienNangAQTongChieuNhan',
          unit: 'chiSoChotDienNangAQTongChieuNhanUnit',
          symbol: 'AQ tổng',
          quanlity: 'chiSoChotDienNangAQTongChieuNhanQuanlity',
          timestamp: 'chiSoChotDienNangAQTongChieuNhanCreatedTime',
        },
      ],
    },

    // Công suất cực đại
    {
      value: [
        {
          attribute: 'pmaxKyHienTai',
          unit: 'pmaxKyHienTaiUnit',
          symbol: 'Công suất cực đại kỳ hiện tại',
          quanlity: 'pmaxKyHienTaiQuanlity',
          timestamp: 'pmaxKyHienTaiCreatedTime',
        },
        {
          attribute: 'pmaxKyTruoc',
          unit: 'pmaxKyTruocUnit',
          symbol: 'Công suất cực đại kỳ trước',
          quanlity: 'pmaxKyTruocQuanlity',
          timestamp: 'pmaxKyTruocCreatedTime',
        },
      ],
    },
    {
      value: [
        {
          symbol: 'Thời điểm ghi nhận',
          attributeComputed: (data: any) => {
            const {
              pmaxCurrentPeriodDate: d,
              pmaxCurrentPeriodMonth: m,
              pmaxCurrentPeriodYear: y,
              pmaxCurrentPeriodTime: h,
              pmaxCurrentPeriodMinute: min,
              pmaxCurrentPeriodSecond: s,
            } = data;
            if (!d || !m || !y) return 'N/A';
            const pad = (v: any) => String(v || 0).padStart(2, '0');
            return `${pad(d)}/${pad(m)}/${y} ${pad(h)}:${pad(min)}:${pad(s)}`;
          },
          timestampComputed: (data: any) => {
            const {
              pmaxCurrentPeriodDate: d,
              pmaxCurrentPeriodMonth: m,
              pmaxCurrentPeriodYear: y,
              pmaxCurrentPeriodTime: h,
              pmaxCurrentPeriodMinute: min,
              pmaxCurrentPeriodSecond: s,
            } = data;
            if (!d || !m || !y) return 'N/A';
            const pad = (v: any) => String(v || 0).padStart(2, '0');
            return `${pad(d)}/${pad(m)}/${y} ${pad(h)}:${pad(min)}:${pad(s)}`;
          },
        },
        {
          symbol: 'Thời điểm ghi nhận',
          attributeComputed: (data: any) => {
            const {
              pmaxPreviousPeriodDate: d,
              pmaxPreviousPeriodMonth: m,
              pmaxPreviousPeriodYear: y,
              pmaxPreviousPeriodTime: h,
              pmaxPreviousPeriodMinute: min,
              pmaxPreviousPeriodSecond: s,
            } = data;
            if (!d || !m || !y) return 'N/A';
            const pad = (v: any) => String(v || 0).padStart(2, '0');
            return `${pad(d)}/${pad(m)}/${y} ${pad(h)}:${pad(min)}:${pad(s)}`;
          },
          timestampComputed: (data: any) => {
            const {
              pmaxPreviousPeriodDate: d,
              pmaxPreviousPeriodMonth: m,
              pmaxPreviousPeriodYear: y,
              pmaxPreviousPeriodTime: h,
              pmaxPreviousPeriodMinute: min,
              pmaxPreviousPeriodSecond: s,
            } = data;
            if (!d || !m || !y) return 'N/A';
            const pad = (v: any) => String(v || 0).padStart(2, '0');
            return `${pad(d)}/${pad(m)}/${y} ${pad(h)}:${pad(min)}:${pad(s)}`;
          },
        },
      ],
    },
  ];

  static GCSsum: Array<any> = [
    { label: 'BT', value: 'BT' },
    { label: 'CD', value: 'CD' },
    { label: 'TD', value: 'TD' },
    { label: 'SG', value: 'SG' },
    { label: 'VC', value: 'VC' },
  ];

  // Data month
  static Months: Array<any> = [
    { label: 'Tháng 1', value: 1 },
    { label: 'Tháng 2', value: 2 },
    { label: 'Tháng 3', value: 3 },
    { label: 'Tháng 4', value: 4 },
    { label: 'Tháng 5', value: 5 },
    { label: 'Tháng 6', value: 6 },
    { label: 'Tháng 7', value: 7 },
    { label: 'Tháng 8', value: 8 },
    { label: 'Tháng 9', value: 9 },
    { label: 'Tháng 10', value: 10 },
    { label: 'Tháng 11', value: 11 },
    { label: 'Tháng 12', value: 12 },
  ];

  // Tag Name list
  static TYPE_TABLE = class {
    static operatingParams = {
      label: 'Thông số vận hành',
      value: 1,
    };

    static powerParams = {
      label: 'Thông số điện năng',
      value: 2,
    };

    static warningParams = {
      label: 'Cảnh báo',
      value: 3,
    };

    static dateTimeParams = {
      label: 'Thời gian',
      value: 4,
    };

    static typeArray: Array<any> = [
      this.operatingParams,
      this.powerParams,
      this.warningParams,
      this.dateTimeParams,
    ];
  };

  static dataMeasurementPoint = class {
    static UNITS = {
      A: [{ label: 'A', value: 'A' }],
      V_KV: [
        { label: 'V', value: 'V' },
        { label: 'kV', value: 'kV' },
      ],
      KW: [{ label: 'kW', value: 'kW' }],
      KVAR: [{ label: 'kVAr', value: 'kVAr' }],
      KWH: [{ label: 'kWh', value: 'kWh' }],
      KVARH: [{ label: 'kVArh', value: 'kVArh' }],
      HZ: [{ label: 'Hz', value: 'Hz' }],
    };

    static dataOperatingParams: Array<any> = [
      {
        label: 'Dòng điện pha A',
        value: 'DONG_DIEN_PHA_A',
        unit: this.UNITS.A,
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Dòng điện pha B',
        value: 'DONG_DIEN_PHA_B',
        unit: this.UNITS.A,
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Dòng điện pha C',
        value: 'DONG_DIEN_PHA_C',
        unit: this.UNITS.A,
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Điện áp pha A',
        value: 'DIEN_AP_PHA_A',
        unit: this.UNITS.V_KV,
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Điện áp pha B',
        value: 'DIEN_AP_PHA_B',
        unit: this.UNITS.V_KV,
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Điện áp pha C',
        value: 'DIEN_AP_PHA_C',
        unit: this.UNITS.V_KV,
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Công suất tác dụng pha A',
        value: 'CONG_SUAT_TAC_DUNG_PHA_A',
        unit: this.UNITS.KW,
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Công suất tác dụng pha B',
        value: 'CONG_SUAT_TAC_DUNG_PHA_B',
        unit: this.UNITS.KW,
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Công suất tác dụng pha C',
        value: 'CONG_SUAT_TAC_DUNG_PHA_C',
        unit: this.UNITS.KW,
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Công suất tác dụng tổng 3 pha',
        value: 'CONG_SUAT_TAC_DUNG_TONG_3_PHA',
        unit: this.UNITS.KW,
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Công suất phản kháng pha A',
        value: 'CONG_SUAT_PHAN_KHANG_PHA_A',
        unit: this.UNITS.KVAR,
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Công suất phản kháng pha B',
        value: 'CONG_SUAT_PHAN_KHANG_PHA_B',
        unit: this.UNITS.KVAR,
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Công suất phản kháng pha C',
        value: 'CONG_SUAT_PHAN_KHANG_PHA_C',
        unit: this.UNITS.KVAR,
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Công suất phản kháng tổng 3 pha',
        value: 'CONG_SUAT_PHAN_KHANG_TONG_3_PHA',
        unit: this.UNITS.KVAR,
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Hệ số công suất PF pha A',
        value: 'HE_SO_CONG_SUAT_PF_PHA_A',
        unit: [],
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Hệ số công suất PF pha B',
        value: 'HE_SO_CONG_SUAT_PF_PHA_B',
        unit: [],
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Hệ số công suất PF pha C',
        value: 'HE_SO_CONG_SUAT_PF_PHA_C',
        unit: [],
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Hệ số công suất PF trung bình 3 pha',
        value: 'HE_SO_CONG_SUAT_PF_TRUNG_BINH_3_PHA',
        unit: [],
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Tần số hệ thống',
        value: 'TAN_SO_HE_THONG',
        unit: this.UNITS.HZ,
        type: 'ELECTRICAL_PARAMETERS',
      },
      {
        label: 'Công suất tác dụng cực đại kỳ hiện tại',
        value: 'PMAX_KY_HIEN_TAI',
        unit: this.UNITS.KW,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Công suất tác dụng cực đại kỳ trước',
        value: 'PMAX_KY_TRUOC',
        unit: this.UNITS.KW,
        type: 'OPERATING_PARAMETERS',
      },
    ];

    static dataPowerParams: Array<any> = [
      {
        label: 'Chỉ số tích lũy điện năng tác dụng AP biểu giờ BT',
        value: 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT',
        unit: this.UNITS.KWH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số tích lũy điện năng tác dụng AP biểu giờ CD',
        value: 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD',
        unit: this.UNITS.KWH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số tích lũy điện năng tác dụng AP biểu giờ TD',
        value: 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD',
        unit: this.UNITS.KWH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số tích lũy điện năng tác dụng AP tổng',
        value: 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG',
        unit: this.UNITS.KWH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số tích lũy điện năng phản kháng AQ tổng',
        value: 'CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG',
        unit: this.UNITS.KVARH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số chốt điện năng AP giờ BT (chỉ số BT)',
        value: 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT',
        unit: this.UNITS.KWH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số chốt điện năng AP giờ CD (chỉ số CD)',
        value: 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD',
        unit: this.UNITS.KWH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số chốt điện năng AP giờ TD (chỉ số TD)',
        value: 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD',
        unit: this.UNITS.KWH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số chốt điện năng AP tổng (chỉ số SG)',
        value: 'CHI_SO_CHOT_DIEN_NANG_AP_TONG',
        unit: this.UNITS.KWH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số chốt điện năng AQ tổng (chỉ số VC)',
        value: 'CHI_SO_CHOT_DIEN_NANG_AQ_TONG',
        unit: this.UNITS.KVARH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số tích lũy điện năng tác dụng AP biểu giờ BT chiều nhận',
        value: 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT_CHIEU_NHAN',
        unit: this.UNITS.KWH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số tích lũy điện năng tác dụng AP biểu giờ CD chiều nhận',
        value: 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD_CHIEU_NHAN',
        unit: this.UNITS.KWH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số tích lũy điện năng tác dụng AP biểu giờ TD chiều nhận',
        value: 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD_CHIEU_NHAN',
        unit: this.UNITS.KWH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số tích lũy điện năng tác dụng AP tổng chiều nhận',
        value: 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG_CHIEU_NHAN',
        unit: this.UNITS.KWH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số tích lũy điện năng phản kháng AQ tổng chiều nhận',
        value: 'CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG_CHIEU_NHAN',
        unit: this.UNITS.KVARH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số chốt điện năng AP giờ BT chiều nhận (chỉ số BT nhận)',
        value: 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT_CHIEU_NHAN',
        unit: this.UNITS.KWH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số chốt điện năng AP giờ CD chiều nhận (chỉ số CD nhận)',
        value: 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD_CHIEU_NHAN',
        unit: this.UNITS.KWH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số chốt điện năng AP giờ TD chiều nhận (chỉ số TD nhận)',
        value: 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD_CHIEU_NHAN',
        unit: this.UNITS.KWH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số chốt điện năng AP tổng chiều nhận (chỉ số SG nhận)',
        value: 'CHI_SO_CHOT_DIEN_NANG_AP_TONG_CHIEU_NHAN',
        unit: this.UNITS.KWH,
        type: 'OPERATING_PARAMETERS',
      },
      {
        label: 'Chỉ số chốt điện năng AQ tổng chiều nhận (chỉ số VC nhận)',
        value: 'CHI_SO_CHOT_DIEN_NANG_AQ_TONG_CHIEU_NHAN',
        unit: this.UNITS.KVARH,
        type: 'OPERATING_PARAMETERS',
      },
    ];

    static dataWarningParams: Array<any> = [
      {
        label: 'Cảnh báo 01',
        value: 'CANH_BAO_01',
        unit: [],
        type: 'WARNING',
      },
      {
        label: 'Cảnh báo 02',
        value: 'CANH_BAO_02',
        unit: [],
        type: 'WARNING',
      },
      {
        label: 'Cảnh báo 03',
        value: 'CANH_BAO_03',
        unit: [],
        type: 'WARNING',
      },
      {
        label: 'Cảnh báo 04',
        value: 'CANH_BAO_04',
        unit: [],
        type: 'WARNING',
      },
      {
        label: 'Cảnh báo 05',
        value: 'CANH_BAO_05',
        unit: [],
        type: 'WARNING',
      },
      {
        label: 'Cảnh báo 06',
        value: 'CANH_BAO_06',
        unit: [],
        type: 'WARNING',
      },
      {
        label: 'Cảnh báo 07',
        value: 'CANH_BAO_07',
        unit: [],
        type: 'WARNING',
      },
      {
        label: 'Cảnh báo 08',
        value: 'CANH_BAO_08',
        unit: [],
        type: 'WARNING',
      },
      {
        label: 'Cảnh báo 09',
        value: 'CANH_BAO_09',
        unit: [],
        type: 'WARNING',
      },
      {
        label: 'Cảnh báo 10',
        value: 'CANH_BAO_10',
        unit: [],
        type: 'WARNING',
      },
      {
        label: 'Trạng thái kết nối',
        value: 'MAT_KET_NOI',
        unit: [],
        type: 'WARNING',
      },
    ];

    static dateTimeParams: Array<any> = [
      // 1. Nhóm Thời gian hiện tại
      {
        label: 'Thời gian hiện tại (năm)',
        subLabel: 'Năm',
        value: 'NAM_HIEN_TAI',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian hiện tại',
      },
      {
        label: 'Thời gian hiện tại (tháng)',
        subLabel: 'Tháng',
        value: 'THANG_HIEN_TAI',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian hiện tại',
      },
      {
        label: 'Thời gian hiện tại (ngày)',
        subLabel: 'Ngày',
        value: 'NGAY_HIEN_TAI',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian hiện tại',
      },
      {
        label: 'Thời gian hiện tại (giờ)',
        subLabel: 'Giờ',
        value: 'GIO_HIEN_TAI',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian hiện tại',
      },
      {
        label: 'Thời gian hiện tại (phút)',
        subLabel: 'Phút',
        value: 'PHUT_HIEN_TAI',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian hiện tại',
      },
      {
        label: 'Thời gian hiện tại (giây)',
        subLabel: 'Giây',
        value: 'GIAY_HIEN_TAI',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian hiện tại',
      },

      // 2. Nhóm Thời gian chốt gần nhất
      {
        label: 'Thời gian chốt gần nhất (năm)',
        subLabel: 'Năm',
        value: 'NAM_CHOT_GAN_NHAT',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian chốt gần nhất',
      },
      {
        label: 'Thời gian chốt gần nhất (tháng)',
        subLabel: 'Tháng',
        value: 'THANG_CHOT_GAN_NHAT',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian chốt gần nhất',
      },
      {
        label: 'Thời gian chốt gần nhất (ngày)',
        subLabel: 'Ngày',
        value: 'NGAY_CHOT_GAN_NHAT',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian chốt gần nhất',
      },
      {
        label: 'Thời gian chốt gần nhất (giờ)',
        subLabel: 'Giờ',
        value: 'GIO_CHOT_GAN_NHAT',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian chốt gần nhất',
      },
      {
        label: 'Thời gian chốt gần nhất (phút)',
        subLabel: 'Phút',
        value: 'PHUT_CHOT_GAN_NHAT',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian chốt gần nhất',
      },
      {
        label: 'Thời gian chốt gần nhất (giây)',
        subLabel: 'Giây',
        value: 'GIAY_CHOT_GAN_NHAT',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian chốt gần nhất',
      },

      // 3. Nhóm Thời gian công suất cực đại kỳ hiện tại
      {
        label: 'Thời gian công suất cực đại kỳ hiện tại (năm)',
        subLabel: 'Năm',
        value: 'NAM_PMAX_KY_HIEN_TAI',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian công suất cực đại kỳ hiện tại',
      },
      {
        label: 'Thời gian công suất cực đại kỳ hiện tại (tháng)',
        subLabel: 'Tháng',
        value: 'THANG_PMAX_KY_HIEN_TAI',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian công suất cực đại kỳ hiện tại',
      },
      {
        label: 'Thời gian công suất cực đại kỳ hiện tại (ngày)',
        subLabel: 'Ngày',
        value: 'NGAY_PMAX_KY_HIEN_TAI',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian công suất cực đại kỳ hiện tại',
      },
      {
        label: 'Thời gian công suất cực đại kỳ hiện tại (giờ)',
        subLabel: 'Giờ',
        value: 'GIO_PMAX_KY_HIEN_TAI',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian công suất cực đại kỳ hiện tại',
      },
      {
        label: 'Thời gian công suất cực đại kỳ hiện tại (phút)',
        subLabel: 'Phút',
        value: 'PHUT_PMAX_KY_HIEN_TAI',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian công suất cực đại kỳ hiện tại',
      },
      {
        label: 'Thời gian công suất cực đại kỳ hiện tại (giây)',
        subLabel: 'Giây',
        value: 'GIAY_PMAX_KY_HIEN_TAI',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian công suất cực đại kỳ hiện tại',
      },

      // 4. Nhóm Thời gian công suất cực đại kỳ trước
      {
        label: 'Thời gian công suất cực đại kỳ trước (năm)',
        subLabel: 'Năm',
        value: 'NAM_PMAX_KY_TRUOC',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian công suất cực đại kỳ trước',
      },
      {
        label: 'Thời gian công suất cực đại kỳ trước (tháng)',
        subLabel: 'Tháng',
        value: 'THANG_PMAX_KY_TRUOC',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian công suất cực đại kỳ trước',
      },
      {
        label: 'Thời gian công suất cực đại kỳ trước (ngày)',
        subLabel: 'Ngày',
        value: 'NGAY_PMAX_KY_TRUOC',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian công suất cực đại kỳ trước',
      },
      {
        label: 'Thời gian công suất cực đại kỳ trước (giờ)',
        subLabel: 'Giờ',
        value: 'GIO_PMAX_KY_TRUOC',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian công suất cực đại kỳ trước',
      },
      {
        label: 'Thời gian công suất cực đại kỳ trước (phút)',
        subLabel: 'Phút',
        value: 'PHUT_PMAX_KY_TRUOC',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian công suất cực đại kỳ trước',
      },
      {
        label: 'Thời gian công suất cực đại kỳ trước (giây)',
        subLabel: 'Giây',
        value: 'GIAY_PMAX_KY_TRUOC',
        unit: [],
        type: 'DATETIME',
        group: 'Thời gian công suất cực đại kỳ trước',
      },
    ];

    static allDataParams: Array<any> = [
      ...this.dataOperatingParams,
      ...this.dataPowerParams,
      ...this.dataWarningParams,
      ...this.dateTimeParams,
    ];
  };

  // list header export file
  static listHeaderExport = class {
    static accounts: Array<any> = [
      { label: 'STT', attr: 'index', width: 4 },
      { label: 'Tên đăng nhập', attr: 'userName', width: 15 },
      { label: 'Nhóm người dùng', attr: 'roleName', width: 15 },
      { label: 'Họ và tên', attr: 'fullName', width: 15 },
      { label: 'Điện thoại', attr: 'phoneNumber', width: 15 },
      { label: 'Email', attr: 'email', width: 26 },
      {
        label: 'Trạng thái',
        attr: 'status',
        width: 10,
        activeMsg: 'Đang hoạt động',
        inactiveMsg: 'Vô hiệu',
      },
    ];

    static clients: Array<any> = [
      { label: 'STT', attr: 'index', width: 4 },
      { label: 'Tên KH', attr: 'name', width: 25 },
      { label: 'Mã KH', attr: 'code', width: 7 },
      { label: 'Số điện thoại', attr: 'phoneNumber', width: 11 },
      { label: 'Email', attr: 'email', width: 25 },
      { label: 'Địa chỉ', attr: 'address', width: 40 },
      {
        label: 'Trạng thái',
        attr: 'status',
        width: 13,
        activeMsg: 'Đang hoạt động',
        inactiveMsg: 'Ngừng hoạt động',
      },
    ];

    static summaryParams: Array<any> = [
      { label: 'Mã điểm đo', attr: 'measurementPointCode', width: 13 },
      { label: 'Mô tả điểm đo', attr: 'measurementPointDescription', width: 13 },
      { label: 'Mã KH', attr: 'customerCode', width: 6 },
      { label: 'Tên KH', attr: 'customerName', width: 28 },
      { label: 'Kết nối', attr: 'matKetNoi', width: 40 / 7 },
      { label: 'U', attr: 'UParams', width: 40 / 7 },
      { label: 'I', attr: 'IParams', width: 40 / 7 },
      { label: 'P', attr: 'PParams', width: 40 / 7 },
      { label: 'Q', attr: 'QParams', width: 40 / 7 },
      { label: 'PF', attr: 'PFParams', width: 40 / 7 },
      { label: 'F', attr: 'FParams', width: 40 / 7 },
    ];

    static warningParams: Array<any> = [
      { label: 'Thời gian', attr: 'createdTime', width: 15 },
      { label: 'Mã điểm đo', attr: 'measurementPointCode', width: 20 },
      { label: 'Tên khách hàng', attr: 'customerName', width: 35 },
      { label: 'Tên cảnh báo', attr: 'eventName', width: 15 },
      { label: 'Trạng thái', attr: 'status', width: 10 },
      { label: 'Giá trị', attr: 'value', width: 5 },
    ];

    static eventParams: Array<any> = [
      { label: 'Thời gian', attr: 'createdTime', width: 15 },
      { label: 'Mã điểm đo', attr: 'measurementPointCode', width: 20 },
      { label: 'Tên khách hàng', attr: 'customerName', width: 35 },
      { label: 'Tên sự kiện', attr: 'eventName', width: 15 },
      { label: 'Trạng thái', attr: 'status', width: 8 },
      { label: 'Giá trị', attr: 'value', width: 7 },
    ];

    static PointManagement: Array<any> = [
      { label: 'STT', attr: 'index', width: 5 },
      { label: 'Mã điểm đo', attr: 'code', width: 11 },
      { label: 'Loại điểm đo', attr: 'type', width: 7 },
      { label: 'Mô tả', attr: 'description', width: 25 },
      { label: 'Tên khách hàng', attr: 'customer', width: 25 },
      { label: 'Email nhận phiếu báo (tối đa 3 email)', attr: 'emails', width: 25 },
      {
        label: 'Trạng thái',
        attr: 'status',
        width: 13,
        activeMsg: 'Đang sử dụng',
        inactiveMsg: 'Ngừng sử dụng',
      },
    ];

    static GcsManagement: Array<any> = [
      { label: 'STT', attr: 'index', width: 3 },
      { label: 'Mã sổ', attr: 'code', width: 15 },
      { label: 'Tên sổ', attr: 'name', width: 15 },
      { label: 'Số kì', attr: 'periodNumber', width: 4 },
      { label: 'Mô tả', attr: 'description', width: 53 },
      {
        label: 'Trạng thái',
        attr: 'status',
        width: 10,
        activeMsg: 'Đang mở',
        inactiveMsg: 'Đã đóng',
      },
    ];
    static GcsCheckDetail: Array<any> = [
      { label: 'Mã điểm đo', attr: 'measurementPoint', width: 13 },
      { label: 'Mô tả', attr: 'description', width: 25 },
      { label: 'Tên khách hàng', attr: 'customer', width: 15 },
      { label: 'Bộ chỉ số', attr: 'meterReadingSet', width: 7 },
      { label: 'Chỉ số cũ', attr: 'previousReading', width: 10 },
      { label: 'Chỉ số mới', attr: 'currentReading', width: 10 },
      { label: 'Hệ số nhân', attr: 'multiplierFactor', width: 10 },
      { label: 'Sản lượng', attr: 'consumption', width: 10 },
    ];

    static GcsSum: Array<any> = [
      { label: 'Mã điểm đo', attr: 'measurementPoint', width: 17 },
      { label: 'Mô tả', attr: 'description', width: 18 },
      { label: 'Tên khách hàng', attr: 'customer', width: 15 },
      { label: 'Bộ chỉ số', attr: 'meterReadingSet', width: 10 },
      { label: 'Chỉ số cũ', attr: 'previousReading', width: 10 },
      { label: 'Chỉ số mới', attr: 'currentReading', width: 10 },
      { label: 'Hệ số nhân', attr: 'multiplierFactor', width: 10 },
      { label: 'Sản lượng', attr: 'consumption', width: 10 },
    ];

    static TotalOutputPoint: Array<any> = [
      { label: 'Mã KH', attr: 'customerCode', width: 10 },
      { label: 'Tên KH', attr: 'customerName', width: 18 },
      { label: 'Mã điểm đo', attr: 'measurementPointCode', width: 13 },
      { label: 'Mô tả điểm đo', attr: 'measurementPointDescription', width: 19 },
      { label: 'Giờ BT', attr: 'bt', width: 10 },
      { label: 'Giờ TĐ', attr: 'td', width: 10 },
      { label: 'Giờ CĐ', attr: 'cd', width: 10 },
      { label: 'Tổng sản lượng', attr: 'sg', width: 10 },
    ];

    static HalfHourFinalizedEnergyReport: Array<any> = [
      { label: 'STT', attr: 'index', width: 4 },
      { label: 'Thời gian', attr: 'createdTime', width: 12 },
      {
        label: 'Kênh giao - Tổng',
        attr: 'chiSoTichLuyDienNangTacDungAPTongKenhGiao',
        width: 10,
      },
      {
        label: 'Kênh giao - Biểu 1 (BT)',
        attr: 'chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao',
        width: 10,
      },
      {
        label: 'Kênh giao - Biểu 2 (CĐ)',
        attr: 'chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao',
        width: 10,
      },
      {
        label: 'Kênh giao - Biểu 3 (TĐ)',
        attr: 'chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao',
        width: 10,
      },
      {
        label: 'Kênh giao - VC (kVArh)',
        attr: 'chiSoTichLuyDienNangPhanKhangAQTongKenhGiao',
        width: 8,
      },
      {
        label: 'Kênh nhận - Tổng',
        attr: 'chiSoTichLuyDienNangTacDungAPTongKenhNhan',
        width: 10,
      },
      {
        label: 'Kênh nhận - Biểu 1 (BT)',
        attr: 'chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan',
        width: 10,
      },
      {
        label: 'Kênh nhận - Biểu 2 (CĐ)',
        attr: 'chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan',
        width: 10,
      },
      {
        label: 'Kênh nhận - Biểu 3 (TĐ)',
        attr: 'chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan',
        width: 10,
      },
      {
        label: 'Kênh nhận - VC (kVArh)',
        attr: 'chiSoTichLuyDienNangPhanKhangAQTongKenhNhan',
        width: 8,
      },
    ];

    static DailyFinalizedEnergyReport: Array<any> = [
      { label: 'STT', attr: 'index', width: 4 },
      { label: 'Ngày', attr: 'createdTime', width: 12 },
      {
        label: 'Kênh giao - Tổng',
        attr: 'chiSoTichLuyDienNangTacDungAPTongKenhGiao',
        width: 10,
      },
      {
        label: 'Kênh giao - Biểu 1 (BT)',
        attr: 'chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao',
        width: 10,
      },
      {
        label: 'Kênh giao - Biểu 2 (CĐ)',
        attr: 'chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao',
        width: 10,
      },
      {
        label: 'Kênh giao - Biểu 3 (TĐ)',
        attr: 'chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao',
        width: 10,
      },
      {
        label: 'Kênh giao - VC (kVArh)',
        attr: 'chiSoTichLuyDienNangPhanKhangAQTongKenhGiao',
        width: 8,
      },
      {
        label: 'Kênh nhận - Tổng',
        attr: 'chiSoTichLuyDienNangTacDungAPTongKenhNhan',
        width: 10,
      },
      {
        label: 'Kênh nhận - Biểu 1 (BT)',
        attr: 'chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan',
        width: 10,
      },
      {
        label: 'Kênh nhận - Biểu 2 (CĐ)',
        attr: 'chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan',
        width: 10,
      },
      {
        label: 'Kênh nhận - Biểu 3 (TĐ)',
        attr: 'chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan',
        width: 10,
      },
      {
        label: 'Kênh nhận - VC (kVArh)',
        attr: 'chiSoTichLuyDienNangPhanKhangAQTongKenhNhan',
        width: 8,
      },
    ];
  };

  //Pattern Regular Expression
  static PATTERN_REGEX = class {
    static Email =
      /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    static AZ_az = /(?=.*[A-Za-z])/;
    static A_Z = /(?=.*[A-Z])/;
    static a_z = /(?=.*[a-z])/;
    static Number = /(?=.*[0-9])/;
    static SpecialCharacter = /(?=.*[!-/:-@[-`{-~])/;

    static PasswordLow = /.{6,}/;
    static PasswordMedium = /(?=.*?[A-Z])(?=.*?[a-z])(?=.*[0-9])(?=.{6,})/;
    static PasswordStrong = /(?=.*?[A-Z])(?=.*?[a-z])(?=.*[0-9])(?=.*[!-/:-@[-`{-~])(?=.{6,})/;

    static space = /(?=.*[A-Za-z])/;

    static CMax_32 = /^.{1,32}$/;

    static PhoneNumber = /^(\+84|0)(3[2-9]|5[689]|7[06-9]|8[1-9]|9[0-4]|9[6-9])[0-9]{7}$/;

    static Link = /^(https?:\/\/)?([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}(:\d+)?(\/[^\s]*)?$/;

    static MinNumber = /^(?:[1-9]\d*|1\d*|[1-9]\d*\.\d+)$/;
  };

  static Color = class {
    static Primary = '#b32322';
  };
}
