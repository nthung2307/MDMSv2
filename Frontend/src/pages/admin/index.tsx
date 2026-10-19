import { Route, Routes, Navigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import Constants from '@/core/application/common/constants';
import MainLayout from '@/infrastructure/common/components/layout/MainLayout';
import { PrivateRoute } from '@/infrastructure/common/components/router/private-router';
import LocalStorageService from '@/infrastructure/services/localStorage.service';
import { getAccountRole } from '@/infrastructure/utils/helpers.ts';
import PermissionDeniedPage from '@/pages/403';
import { NotFound } from '@/pages/404';
import AddAccount from '@/pages/account-management/add-account';
import AccountDetail from '@/pages/account-management/detail';
import EditAccount from '@/pages/account-management/edit-account';
import AccountManagement from '@/pages/account-management/index';
import AddClient from '@/pages/client-management/add-client/index';
import ClientDetail from '@/pages/client-management/detail';
import EditClient from '@/pages/client-management/edit-client';
import ClientManagement from '@/pages/client-management/index';
import Configuration from '@/pages/config';
import AuthLock from '@/pages/config/auth-lock';
import ContactConfig from '@/pages/config/contact';
import DataMeasurementPoint from '@/pages/config/detail';
import DashBoardPage from '@/pages/dashboard';
import AddGCS from '@/pages/gcs-management/add';
import GCSCheck from '@/pages/gcs-management/check';
import GCSCheckDetail from '@/pages/gcs-management/check-detail';
import GCSDetails from '@/pages/gcs-management/detail/index';
import EditGCS from '@/pages/gcs-management/edit';
import GCSManagement from '@/pages/gcs-management/index';
import GCSSum from '@/pages/gcs-sum/index';
import NoticeManagement from '@/pages/notice-management';
import NoticeDetail from '@/pages/notice-management/notice-detail';
import AddPoint from '@/pages/point-management/add-point';
import PointDetail from '@/pages/point-management/detail';
import EditPoint from '@/pages/point-management/edit-client';
import PointManagement from '@/pages/point-management/index';
import PointMonitoringEvent from '@/pages/point-monitoring/point-event';
import PointMonitoringWarning from '@/pages/point-monitoring/point-warning';
import PointMonitoring from '@/pages/point-monitoring/summary-parameter';
import PointMonitoringDetail from '@/pages/point-monitoring/summary-parameter/detail';
import Profile from '@/pages/profile';
import EditProfile from '@/pages/profile/edit-profile/edit-profile.tsx';
import ReportManagement from '@/pages/report-management/index';
import DailyFinalizedEnergyReport from '@/pages/reports/daily-finalized-energy-report/daily-finalized-energy-report.tsx';
import electricityConsumption from '@/pages/reports/electricity-consumption/electricity-consumption.tsx';
import HalfHourFinalizedEnergyReport from '@/pages/reports/half-hour-finalized-energy-report/half-hour-finalized-energy-report.tsx';
import LoadChart from '@/pages/reports/load-curve/load-curve.tsx';
import operatingParameter from '@/pages/reports/operating-parameter/operating-parameter.tsx';
import totalOutputMeasurementPoints from '@/pages/reports/total-output-measurement-points/total-output-measurement-points.tsx';
import YearlyFinalizedEnergyReport from '@/pages/reports/yearly-finalized-energy-report/yearly-finalized-energy-report.tsx';
import ActivePointsEnergyReport from '@/pages/reports/active-points-energy-report/active-points-energy-report.tsx';

export const AdminPage = () => {
  // 👇️ conditionally redirect based on isAuthenticated boolean
  const AuthWrapper = () => {
    let localStorageService = new LocalStorageService();
    let accessToken = localStorageService.readStorage(Constants.API_TOKEN_STORAGE);
    if (accessToken) {
      switch (getAccountRole()) {
        case Constants.Role.Customer.value:
          return <Navigate to={ROUTE_PATH.OPERATING_PARAMETER} />;
        case Constants.Role.Staff.value:
        case Constants.Role.Admin.value:
        case Constants.Role.SuperAdmin.value:
          return <Navigate to={ROUTE_PATH.POINT_MANAGEMENT} />;
        default:
          return <Navigate to={ROUTE_PATH.FORBIDDEN_ACCESS} />;
      }
    } else {
      return <Navigate to={ROUTE_PATH.LOGIN} replace={true} />;
    }
  };

  return (
    <div>
      <MainLayout>
        <Routes>
          <Route path='*' element={<NotFound />} />
          <Route path='/' element={<AuthWrapper />} />
          <Route path={ROUTE_PATH.FORBIDDEN_ACCESS} element={<PermissionDeniedPage />} />
          <Route
            path={ROUTE_PATH.DASHBOARD}
            element={
              <PrivateRoute
                component={DashBoardPage}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Customer.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />

          {/*Profile*/}
          <Route
            path={ROUTE_PATH.USER_PROFILE}
            element={
              <PrivateRoute
                component={Profile}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Customer.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.EDIT_USER_PROFILE}
            element={
              <PrivateRoute
                component={EditProfile}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Customer.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          {/* End of Profile*/}

          {/* Account Management */}
          <Route
            path={ROUTE_PATH.ACCOUNT_MANAGEMENT}
            element={
              <PrivateRoute
                component={AccountManagement}
                roles={[Constants.Role.SuperAdmin.value]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.ADD_ACCOUNT}
            element={
              <PrivateRoute component={AddAccount} roles={[Constants.Role.SuperAdmin.value]} />
            }
          />
          <Route
            path={ROUTE_PATH.EDIT_ACCOUNT}
            element={
              <PrivateRoute component={EditAccount} roles={[Constants.Role.SuperAdmin.value]} />
            }
          />
          <Route
            path={ROUTE_PATH.ACCOUNT_DETAIL}
            element={
              <PrivateRoute component={AccountDetail} roles={[Constants.Role.SuperAdmin.value]} />
            }
          />
          {/* End of Account Management */}

          {/* Customer Management */}
          <Route
            path={ROUTE_PATH.CUSTOMER_MANAGEMENT}
            element={
              <PrivateRoute
                component={ClientManagement}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.ADD_CUSTOMER}
            element={
              <PrivateRoute
                component={AddClient}
                roles={[Constants.Role.SuperAdmin.value, Constants.Role.Admin.value]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.EDIT_CUSTOMER}
            element={
              <PrivateRoute
                component={EditClient}
                roles={[Constants.Role.SuperAdmin.value, Constants.Role.Admin.value]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.CUSTOMER_DETAIL}
            element={
              <PrivateRoute
                component={ClientDetail}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          {/* End of Client Management */}

          {/* Point Monitoring */}
          <Route
            path={ROUTE_PATH.POINT_MONITORING}
            element={
              <PrivateRoute
                component={PointMonitoring}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.POINT_MONITORING_DETAIL}
            element={
              <PrivateRoute
                component={PointMonitoringDetail}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.POINT_MONITORING_EVENT}
            element={
              <PrivateRoute
                component={PointMonitoringEvent}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.POINT_MONITORING_WARNING}
            element={
              <PrivateRoute
                component={PointMonitoringWarning}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          {/* End of Point Monitoring */}

          {/* Reports */}
          <Route
            path={ROUTE_PATH.LOAD_CURVE}
            element={
              <PrivateRoute
                component={LoadChart}
                roles={[
                  Constants.Role.Admin.value,
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Customer.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.ELECTRICITY_CONSUMPTION}
            element={
              <PrivateRoute
                component={electricityConsumption}
                roles={[
                  Constants.Role.Admin.value,
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Customer.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.OPERATING_PARAMETER}
            element={
              <PrivateRoute
                component={operatingParameter}
                roles={[
                  Constants.Role.Admin.value,
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Customer.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.TOTAL_OUTPUT_MEASUREMENT_POINTS}
            element={
              <PrivateRoute
                component={totalOutputMeasurementPoints}
                roles={[
                  Constants.Role.Admin.value,
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Customer.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.FINALIZED_ENERGY_REPORT}
            element={<Navigate to={ROUTE_PATH.HALF_HOUR_FINALIZED_ENERGY_REPORT} replace />}
          />
          <Route
            path={ROUTE_PATH.HALF_HOUR_FINALIZED_ENERGY_REPORT}
            element={
              <PrivateRoute
                component={HalfHourFinalizedEnergyReport}
                roles={[
                  Constants.Role.Admin.value,
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Customer.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.DAILY_FINALIZED_ENERGY_REPORT}
            element={
              <PrivateRoute
                component={DailyFinalizedEnergyReport}
                roles={[
                  Constants.Role.Admin.value,
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Customer.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.YEARLY_FINALIZED_ENERGY_REPORT}
            element={
              <PrivateRoute
                component={YearlyFinalizedEnergyReport}
                roles={[
                  Constants.Role.Admin.value,
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Customer.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.ACTIVE_POINTS_ENERGY_REPORT}
            element={
              <PrivateRoute
                component={ActivePointsEnergyReport}
                roles={[
                  Constants.Role.Admin.value,
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Customer.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          {/* End of Reports */}

          {/* Point Management */}
          <Route
            path={ROUTE_PATH.POINT_MANAGEMENT}
            element={
              <PrivateRoute
                component={PointManagement}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.ADD_POINT}
            element={
              <PrivateRoute
                component={AddPoint}
                roles={[Constants.Role.SuperAdmin.value, Constants.Role.Admin.value]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.POINT_DETAIL}
            element={
              <PrivateRoute
                component={PointDetail}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.EDIT_POINT}
            element={
              <PrivateRoute
                component={EditPoint}
                roles={[Constants.Role.SuperAdmin.value, Constants.Role.Admin.value]}
              />
            }
          />
          {/* End of Point Management */}

          {/* GCS Management */}
          <Route
            path={ROUTE_PATH.GCS_MANAGEMENT}
            element={
              <PrivateRoute
                component={GCSManagement}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.GCS_DETAILS}
            element={
              <PrivateRoute
                component={GCSDetails}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.EDIT_GCS}
            element={
              <PrivateRoute
                component={EditGCS}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.ADD_GCS}
            element={
              <PrivateRoute
                component={AddGCS}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.GCS_CHECK}
            element={
              <PrivateRoute
                component={GCSCheck}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.GCS_CHECK_DETAIL}
            element={
              <PrivateRoute
                component={GCSCheckDetail}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          {/* End of GCS Management */}

          {/* Report Management */}
          <Route
            path={ROUTE_PATH.REPORT_MANAGEMENT}
            element={<PrivateRoute component={ReportManagement} roles={[]} />}
          />
          {/* End of Report Management */}
          {/* GCS Sum */}
          <Route
            path={ROUTE_PATH.GCS_SUM}
            element={
              <PrivateRoute
                component={GCSSum}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          {/* End of GCS Sum */}
          {/* Notice Management */}
          <Route
            path={ROUTE_PATH.NOTICE_MANAGEMENT}
            element={
              <PrivateRoute
                component={NoticeManagement}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.NOTICE_DETAIl}
            element={
              <PrivateRoute
                component={NoticeDetail}
                roles={[
                  Constants.Role.SuperAdmin.value,
                  Constants.Role.Admin.value,
                  Constants.Role.Staff.value,
                ]}
              />
            }
          />
          {/* End of Notice Management */}
          {/* Configuration */}
          <Route
            path={ROUTE_PATH.CONFIGURATION}
            element={
              <PrivateRoute component={Configuration} roles={[Constants.Role.SuperAdmin.value]} />
            }
          />
          <Route
            path={ROUTE_PATH.DATA_MEASUREMENT_POINT}
            element={
              <PrivateRoute
                component={DataMeasurementPoint}
                roles={[Constants.Role.SuperAdmin.value]}
              />
            }
          />
          <Route
            path={ROUTE_PATH.AUTH_LOCK}
            element={
              <PrivateRoute component={AuthLock} roles={[Constants.Role.SuperAdmin.value]} />
            }
          />
          <Route
            path={ROUTE_PATH.CONTACT}
            element={
              <PrivateRoute component={ContactConfig} roles={[Constants.Role.SuperAdmin.value]} />
            }
          />
        </Routes>
      </MainLayout>
    </div>
  );
};
