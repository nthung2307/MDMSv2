export class Endpoint {
  static Auth = class {
    static Login: string = 'auth/sign-in';
    static RefreshToken: string = 'auth/refresh-token';
    static ForgotPassword: string = 'auth/forgot-password';
    static ConfirmForgotPassword: string = 'auth/confirm-forgot-password';
    static Logout: string = 'auth/logout';
    static ChangeInformationAccount = 'auth/change-information-account';
    static ChangePassword = 'auth/change-password';
    static CheckTokenForgotPassword = 'auth/check-token-forgot-password';
  };

  static Account = class {
    static GetListAccounts: string = 'user/get-list-users';
    static GetAccountDetail: string = 'user/get-user-detail';
    static UpdateAccount: string = 'user/update-user';
    static CreateAccount: string = 'user/create-user';
    static DeleteAccount: string = 'user/delete-user/:id';
    static UpdateStatus: string = 'user/change-status';
  };
  static GCS = class {
    static GetListGCS: string = 'gcs/get-list-gcs';
    static GetGCSDetail: string = 'gcs/get-gcs';
    static CreateGCS: string = 'gcs/create-gcs';
    static UpdateGCS: string = 'gcs/update-gcs';
    static DeleteGCS: string = 'gcs/delete-gcs/:id';
    static UpdateGCSStatus: string = 'gcs/change-status-gcs';
    static UpdateGCSTemporary: string = 'gcs/update-gcs-temporary';
    static FinalMeterReading: string = 'gcs/final-meter-reading';
    static GCSCutOffCycle: string = 'gcs-cutoff-cycle/get-list-gcs-cutoff-cycle';
    static GCSYearCutOffCycle: string = 'gcs-cutoff-cycle/get-list-year-gcs-cutoff-cycle';
    static GCSMonthlyDetail: string = 'gcs-monthly-detail/get-list-gcs-monthly-detail';
  };
  static Notice = class {
    static GetListNotice: string = 'periodic-energy-statement/get-list-periodic-energy-statement';
    static GetStatusSendMail: string = 'periodic-energy-statement/check-status-send-mail';
    static SendMailNotice: string = 'periodic-energy-statement/send-mail';
    static SendAllMailNotice: string = 'periodic-energy-statement/send-all-mail';
  };
  static Customer = class {
    static GetListCustomer: string = 'customers/get-list-customers';
    static GetCustomerDetail: string = 'customers/get-customer-detail';
    static CreateCustomer: string = 'customers/create-customer';
    static UpdateCustomer: string = 'customers/update-customer';
    static DeleteCustomer: string = 'customers/delete-customer/:id';
    static ChangeCustomerStatus: string = 'customers/change-status';
    static GetListCustomerForReport: string = 'customers/get-list-customers-for-report';
    static ImportCustomerCsv: string = 'customers/import-csv';
  };

  static IoT = class {
    static GetData: string = 'iot/get-data-measurement-point-monitoring';
    static GetListGCS: string = 'iot/get-data-gcs';
  };

  static MeasurementPoint = class {
    static CreateMeasurementPoint: string = 'measurement-points/create-measurement-point';
    static UpdateMeasurementPoint: string = 'measurement-points/update-measurement-point';
    static DeleteMeasurementPoint: string = 'measurement-points/delete-measurement-point/:id';
    static GetListMeasurementPoints: string = 'measurement-points/get-list-measurement-points';
    static GetMeasurementPointDetail: string = 'measurement-points/get-measurement-point';
    static ChangeMeasurementPointStatus: string =
      'measurement-points/change-status-measurement-point';
    static GetCountAlert: string = 'measurement-point-monitoring/cout-alert';
    static GetListMeasurementPointForReport: string =
      'measurement-points/get-list-measurement-points-for-report';
    static ImportMeasurementPointCsv: string = 'measurement-points/import-csv';
  };

  static MeasurementPointMonitoring = class {
    static GetListMeasurementPointMonitoring: string =
      'measurement-point-monitoring/get-list-measurement-point-monitoring';
    static GetListMeasurementPointEvent: string =
      'measurement-point-monitoring/get-list-measurement-point-event';
    static GetListMeasurementPointAlert: string =
      'measurement-point-monitoring/get-list-measurement-point-alert';
    static GetMeasurementPointMonitoring: string =
      'measurement-point-monitoring/get-measurement-point-monitoring';
  };

  static Config = class {
    static GetTagNameList: string = 'tag-name/get-list-tag-name';
    static CreateTagName: string = 'tag-name/create-tag-name';

    static GetSystemList: string = 'system-config/get-list-system-config';
    static CreateSystemConfig: string = 'system-config/create-update-system-config';
  };

  static Report = class {
    static GetLoadChart: string = 'report/load-chart';
    static GetVoltageChart: string = 'report/voltage-chart';
    static GetPowerChart: string = 'report/power-chart';
    static GetConsumptionOutputChart: string = 'report/consumption-output-chart';
    static GetFinalizedOutputElectricityChart: string = 'report/finalized-output-electricity-chart';
    static GetListFinalizedOutputElectricity: string = 'report/list-finalized-output-electricity';
    static GetHalfHourFinalizedEnergyReport: string = 'report/half-hour-finalized-energy-report';
    static GetDailyFinalizedEnergyReport: string = 'report/daily-finalized-energy-report';
    static GetYearlyFinalizedEnergyReport: string = 'report/yearly-finalized-energy-report';

    static ExportCsvHalfHourFinalizedEnergy: string = 'half-hour-finalized-energy/export-csv-half-hour-finalized-energy';
    static ExportPdfHalfHourFinalizedEnergy: string = 'half-hour-finalized-energy/export-pdf-half-hour-finalized-energy';
    static ExportExcelHalfHourFinalizedEnergy: string = 'half-hour-finalized-energy/export-excel-half-hour-finalized-energy';
    static ExportCsvDailyFinalizedEnergy: string = 'daily-finalized-energy/export-csv-daily-finalized-energy';
    static ExportPdfDailyFinalizedEnergy: string = 'daily-finalized-energy/export-pdf-daily-finalized-energy';
    static ExportExcelDailyFinalizedEnergy: string = 'daily-finalized-energy/export-excel-daily-finalized-energy';
    static ExportCsvYearlyFinalizedEnergy: string = 'yearly-finalized-energy/export-csv-yearly-finalized-energy';
    static ExportPdfYearlyFinalizedEnergy: string = 'yearly-finalized-energy/export-pdf-yearly-finalized-energy';
    static ExportExcelYearlyFinalizedEnergy: string = 'yearly-finalized-energy/export-excel-yearly-finalized-energy';

    // Báo cáo thông số vận hành 30 phút
    static GetOperatingParameterReport: string = 'report/operating-parameter-report';
    static GetOperatingParameterChart: string = 'report/operating-parameter-chart';
    static ExportCsvOperatingParameter: string = 'report/export-csv-operating-parameter';
    static ExportPdfOperatingParameter: string = 'report/export-pdf-operating-parameter';
    static ExportExcelOperatingParameter: string = 'report/export-excel-operating-parameter';

    // Báo cáo sản lượng điện
    static GetEnergyConsumptionReport: string = 'report/energy-consumption-report';
    static ExportCsvEnergyConsumption: string = 'report/export-csv-energy-consumption';
    static ExportPdfEnergyConsumption: string = 'report/export-pdf-energy-consumption';
    static ExportExcelEnergyConsumption: string = 'report/export-excel-energy-consumption';
  };

  static Export = class {
    static ExportAllPdf: string = 'export-pdf';
    static ExportAllCsv: string = 'export-csv';
  };
}
