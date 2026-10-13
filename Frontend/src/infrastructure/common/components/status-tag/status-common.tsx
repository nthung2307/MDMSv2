import Constants from '@/core/application/common/constants';
import StatusTagCommon from '@/infrastructure/common/components/status-tag/status-tag-common';

export const renderStatus = (record: any) => {
  switch (record.status) {
    case Constants.AccountStatus.Active:
      return <StatusTagCommon status={Constants.AccountStatus.Active} />;
    case Constants.AccountStatus.Deactivate:
      return <StatusTagCommon status={Constants.AccountStatus.Deactivate} bgColor='#0000004D' />;
    default:
      return '';
  }
};
