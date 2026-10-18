const PREFIX: string = '';

export const ROUTE_PATH = {
  LOGIN: `${PREFIX}/login`,
  LOGOUT: `${PREFIX}/logout`,
  FORGOT_PASSWORD: `${PREFIX}/forgot-password`,
  RESET_PASSWORD: `${PREFIX}/reset-password`,
  RESET_ERROR: `${PREFIX}/reset-password/error`,
  RESET_SUCCESS: `${PREFIX}/reset-password/success`,
  DASHBOARD: `${PREFIX}/dashboard`,
  USER_PROFILE: `${PREFIX}/profile`,
  EDIT_USER_PROFILE: `${PREFIX}/profile/edit`,

  // Account Management
  ACCOUNT_MANAGEMENT: `${PREFIX}/accounts`,
  ADD_ACCOUNT: `${PREFIX}/accounts/add`,
  EDIT_ACCOUNT: `${PREFIX}/accounts/:id/edit`,
  ACCOUNT_DETAIL: `${PREFIX}/accounts/:id/detail`,
  // End of Account Management

  // Customer Management
  CUSTOMER_MANAGEMENT: `${PREFIX}/customers`,
  ADD_CUSTOMER: `${PREFIX}/customers/add`,
  EDIT_CUSTOMER: `${PREFIX}/customers/:id/edit`,
  CUSTOMER_DETAIL: `${PREFIX}/customers/:id/detail`,
  // End of Customer Management

  // Measurement point Management
  POINT_MANAGEMENT: `${PREFIX}/point-management`,
  ADD_POINT: `${PREFIX}/point-management/add`,
  EDIT_POINT: `${PREFIX}/point-management/:id/edit`,
  POINT_DETAIL: `${PREFIX}/point-management/:id/detail`,
  // End of Measurement point Management

  POINT_MONITORING: `${PREFIX}/monitoring-points/list`,
  POINT_MONITORING_EVENT: `${PREFIX}/monitoring-points/event`,
  POINT_MONITORING_WARNING: `${PREFIX}/monitoring-points/warning`,
  POINT_MONITORING_DETAIL: `${PREFIX}/monitoring-points/list/:id/detail`,

  // Report
  REPORTS: `${PREFIX}/reports`,
  LOAD_CURVE: `${PREFIX}/reports/load-curve`,
  ELECTRICITY_CONSUMPTION: `${PREFIX}/reports/electricity-consumption`,
  TOTAL_OUTPUT_MEASUREMENT_POINTS: `${PREFIX}/reports/total-output-measurement-points`,
  OPERATING_PARAMETER: `${PREFIX}/reports/operating-parameter`,
  FINALIZED_ENERGY_REPORT: `${PREFIX}/reports/finalized-energy`,
  HALF_HOUR_FINALIZED_ENERGY_REPORT: `${PREFIX}/reports/finalized-energy/interval`,
  DAILY_FINALIZED_ENERGY_REPORT: `${PREFIX}/reports/finalized-energy/monthly`,
  YEARLY_FINALIZED_ENERGY_REPORT: `${PREFIX}/reports/finalized-energy/yearly`,
  // End of Report

  // GCS Management
  GCS_MANAGEMENT: `${PREFIX}/gcs-management`,
  GCS_DETAILS: `${PREFIX}/gcs-management/:id/detail`,
  GCS_CHECK: `${PREFIX}/gcs-management/check`,
  GCS_CHECK_DETAIL: `${PREFIX}/gcs-management/check-detail`,
  EDIT_GCS: `${PREFIX}/gcs-management/:id/edit`,
  ADD_GCS: `${PREFIX}/gcs-management/add`,
  GCS_SUM: `${PREFIX}/gcs-latch`,
  // End of GCS Management

  REPORT_MANAGEMENT: `${PREFIX}/report-management`,
  FORBIDDEN_ACCESS: `${PREFIX}/forbidden-access`,

  NOTICE_DETAIl: `${PREFIX}/notice-management/detail`,

  NOTICE_MANAGEMENT: `${PREFIX}/notice-management`,

  // config
  CONFIGURATION: `${PREFIX}/configuration`,
  AUTH_LOCK: `${PREFIX}/auth-lock`,
  CONTACT: `${PREFIX}/contact`,
  DATA_MEASUREMENT_POINT: `${PREFIX}/configuration/list-tag-name/:id`,
};
