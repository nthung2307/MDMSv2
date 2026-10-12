import { ToastPosition } from 'react-toastify';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import Menu from '@/core/application/common/models/left-menu/Menu';
import MenuItem from '@/core/application/common/models/left-menu/MenuItem';
import { MenuKeys } from '@/core/domain/enums/MenuKeys';
import { RoleId } from '@/core/domain/enums/Role';
export default class Constants {
  //static API_BASE_URL = 'http://192.168.100.10:12704';s
  //static API_BASE_URL = 'https://localhost:12704';
  static API_TOKEN_STORAGE: string = 'QS_ADMIN_API_TOKEN';
  static API_ROLE_STORAGE: string = 'QS_ADMIN_API_ROLE';

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
    static idInType = ':type/:id';
    static category = ':category';
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

  static MenuConfigs = class {
    static DashboardMenu = 'dashboard';
    static MainMenu = new Menu('light', 'main-menu', [MenuKeys.MainMenu], 'inline', [
      new MenuItem(
        [],
        MenuKeys.Dashboard,
        null,
        'Dashboard',
        'Dashboard',
        [RoleId.Admin],
        ROUTE_PATH.DASHBOARD,
        null
      ),
    ]);
  };
  static Logger = class {
    static DateTimeFormat = 'yyyy-MM-DD HH:mm:ss';
    static DateFormat = 'yyyy-MM-DD';
  };
  static DateTime = class {
    static DateTimeFormat = 'yyyy-MM-DD HH:mm:ss.SSSS';
    static DateFormat = 'DD/MM/YYYY';
  };

  static LocalStorage = class {
    static LeftMenu: string = 'LEFT_MENU';
    static ApiToken: string = 'QS_ADMIN_API_TOKEN';
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

  //Pattern Regular Expression
  static PATTERN_REGEX = class {
    static Email =
      /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
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
  };
}
