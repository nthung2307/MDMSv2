import { TableProps, Tooltip, Tag } from 'antd';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants.ts';
import { ActionDetailLink } from '@/infrastructure/common/components/context-menu/ActionDetailLink';
import TableCommon from '@/infrastructure/common/components/table/table-common';
import { buildPointMonitoringDetailUrl } from '@/infrastructure/utils/helpers';

interface Props {
  dataSource: any;
  loading?: boolean;
}

const TableSummary = (props: Props) => {
  const { dataSource, loading } = props;

  const renderValue = (val: number | null | undefined, quanlity: boolean, unit?: any) => {
    if (val === null || val === undefined) return `-`;

    const isBad = quanlity === false;

    if (isBad) {
      return (
        <Tooltip title="Số liệu nghi ngờ (Bad quality)">
          <span className='inline-flex items-center gap-1 text-amber-700 font-medium cursor-help'>
            <span>{`${val} ${unit ? unit : ''}`.trim()}</span>
            <span className='inline-flex items-center justify-center w-3.5 h-3.5 text-[10px] font-bold text-white bg-red-500 rounded-full'>
              !
            </span>
          </span>
        </Tooltip>
      );
    }

    return <span>{`${val} ${unit ? unit : ''}`.trim()}</span>;
  };

  const columns: TableProps['columns'] = [
    {
      title: 'Mã điểm đo',
      dataIndex: 'measurementPointCode',
      fixed: true,
      width: '160px',
      className: 'antd-action-table',
      render: (val, record) => {
        return (
          <div>
            <span className='font-semibold'>{val || 'N/A'}</span>
            {record.meterSerial && (
              <div className='text-xs text-gray-400 truncate' title={`Serial: ${record.meterSerial}`}>
                SN: {record.meterSerial}
              </div>
            )}
          </div>
        );
      },
    },
    // {
    //   title: 'Mã KH',
    //   dataIndex: 'customerCode',
    //   className: 'antd-action-table',
    //   fixed: true,
    //   width: 150,
    //   render: (record) => {
    //     return record ? <span>{record}</span> : <span>N/A</span>;
    //   },
    // },
    {
      title: 'Tên khách hàng',
      dataIndex: 'customerName',
      width: 200,
      render: (record) => {
        return (
          <Tooltip title={`${record}`}>
            <div className='w-52 truncate'>{record ? <span>{record}</span> : <span>N/A</span>}</div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Kết nối đo đếm',
      width: 130,
      dataIndex: 'matKetNoi',
      align: 'center',
      render: (val) => {
        if (val === null || val === undefined) {
          return <span>-</span>;
        }
        return val ? (
          <Tooltip title='Mất tín hiệu thu thập từ công tơ đo đếm'>
            <Tag color='error' className='cursor-help'>Mất kết nối</Tag>
          </Tooltip>
        ) : (
          <Tooltip title='Kênh thu thập công tơ hoạt động bình thường'>
            <Tag color='success' className='cursor-help'>Bình thường</Tag>
          </Tooltip>
        );
      },
    },
    {
      title: 'Kết nối IoT',
      width: 130,
      dataIndex: 'restStatus',
      align: 'center',
      render: (val, record) => {
        const reason = record.restStatusReason || '';
        if (val === 'ONLINE') {
          return (
            <Tooltip title={reason || 'Kênh truyền IoT hoạt động bình thường'}>
              <Tag color='cyan' className='cursor-help'>Online</Tag>
            </Tooltip>
          );
        }
        if (val === 'STALE_STOPPED') {
          return (
            <Tooltip title={reason || 'Kênh truyền IoT bị gián đoạn / ngừng cập nhật'}>
              <Tag color='warning' className='cursor-help font-medium'>Ngừng nhận</Tag>
            </Tooltip>
          );
        }
        if (val === 'NEVER_RECEIVED') {
          return (
            <Tooltip title={reason || 'Chưa từng nhận dữ liệu qua kênh truyền IoT (Kiểm tra Gateway / Key)'}>
              <Tag color='error' className='cursor-help font-medium'>Chưa nhận</Tag>
            </Tooltip>
          );
        }
        return <span>-</span>;
      },
    },
    {
      title: 'U',
      align: 'center',
      width: 100,
      dataIndex: 'dienApPhaA',
      render: (val, record) => {
        return <span>{renderValue(val, record.dienApPhaAQuanlity, record.dienApPhaAUnit)}</span>;
      },
    },
    {
      title: 'I',
      align: 'center',
      width: 100,
      dataIndex: 'dongDienPhaA',
      render: (val, record) => {
        return (
          <span>{renderValue(val, record.dongDienPhaAQuanlity, record.dongDienPhaAUnit)}</span>
        );
      },
    },
    {
      title: 'P',
      align: 'center',
      width: 100,
      dataIndex: 'congSuatTacDungTong3Pha',
      render: (val, record) => {
        return (
          <span>
            {renderValue(
              val,
              record.congSuatTacDungTong3PhaQuanlity,
              record.congSuatTacDungTong3PhaUnit
            )}
          </span>
        );
      },
    },
    {
      title: 'Q',
      align: 'center',
      width: 100,
      dataIndex: 'congSuatPhanKhangTong3Pha',
      render: (val, record) => {
        return (
          <span>
            {renderValue(
              val,
              record.congSuatPhanKhangTong3PhaQuanlity,
              record.congSuatPhanKhangTong3PhaUnit
            )}
          </span>
        );
      },
    },
    {
      title: 'PF',
      align: 'center',
      width: 100,
      dataIndex: 'heSoCongSuatPFTrungBinh3Pha',
      render: (val, record) => {
        return <span>{renderValue(val, record.heSoCongSuatPFTrungBinh3PhaQuanlity)}</span>;
      },
    },
    {
      title: 'F',
      align: 'center',
      width: 100,
      dataIndex: 'tanSoHeThong',
      render: (val, record) => {
        return <span>{renderValue(val, record.tanSoHeThong, record.tanSoHeThongUnit)} </span>;
      },
    },
    {
      title: 'Thao tác',
      key: 'action',
      fixed: 'right',
      align: 'center',
      width: 90,
      render: (_, record) => {
        const url = buildPointMonitoringDetailUrl(
          ROUTE_PATH.POINT_MONITORING_DETAIL,
          Constants.ReplaceRouterConfig.id,
          record.measurementPointCode
        );
        return (
          <ActionDetailLink
            href={url}
            title={`Chi tiết điểm đo: ${record.measurementPointName || record.measurementPointCode}`}
          />
        );
      },
    },
  ];

  return (
    <TableCommon
      rowKey={(record: any) =>
        record.measurementPointsId || record.measurementPointCode || record.id || record.meterSerial
      }
      columns={columns}
      dataSource={dataSource}
      pagination={false}
      rowClassName={(record) => {
        if (record.matKetNoi === true) {
          return 'error-row cursor-pointer';
        }
        if (record.restStatus === 'NEVER_RECEIVED') {
          return 'bg-rose-50/50 cursor-pointer';
        }
        if (record.restStatus === 'STALE_STOPPED') {
          return 'bg-amber-50/50 cursor-pointer';
        }
        return 'cursor-pointer';
      }}
      loading={loading}
      scroll={{ x: 'max-content', y: window.innerHeight * 0.66 }}
    />
  );
};

export default TableSummary;
