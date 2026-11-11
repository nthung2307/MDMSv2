import React, { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import {
  Card,
  Button,
  Tag,
  Space,
  Tooltip,
  Popconfirm,
  Pagination,
} from 'antd';
import {
  PlusOutlined,
  EyeOutlined,
  EditOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  DeleteOutlined,
} from '@ant-design/icons';
import { IoSearch, IoBackspaceOutline } from 'react-icons/io5';
import dayjs from 'dayjs';

import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common.tsx';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import TableCommon from '@/infrastructure/common/components/table/table-common.tsx';
import PrivateComponent from '@/infrastructure/common/components/private-component/private-component.tsx';
import { notifyError, notifySuccess } from '@/infrastructure/common/components/toast/toastMessage.tsx';
import useMeterReplacementHook from '@/infrastructure/repository/meter-replacement/hooks/useMeterReplacementHook.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import Constants from '@/core/application/common/constants.ts';
import { _t } from '@/infrastructure/utils/translation.ts';
import { ReplacementModal } from '@/pages/meter-replacement/components/replacement-modal.tsx';

const MeterReplacementPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const queryPointId = searchParams.get('pointId');

  const [loading, setLoading] = useState<boolean>(false);
  const [dataList, setDataList] = useState<any[]>([]);
  const [total, setTotal] = useState<number>(0);
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);

  // Form bộ lọc (Mã điểm đo, Tên khách hàng, Trạng thái)
  const [dataForm, setDataForm] = useStateDataForm({
    pointCode: '',
    customerName: '',
    status: '',
  });

  // Modal lập/sửa/xem biên bản
  const [modalVisible, setModalVisible] = useState<boolean>(false);
  const [modalMode, setModalMode] = useState<'create' | 'edit' | 'view'>('create');
  const [selectedRecord, setSelectedRecord] = useState<any>(null);

  const { getList, approveReplacement, cancelReplacement, deleteReplacement } = useMeterReplacementHook();

  // Debounce tìm kiếm text (300ms) tương tự các trang khác
  const debouncePointCode = useDebounce(dataForm?.pointCode, 300);
  const debounceCustomerName = useDebounce(dataForm?.customerName, 300);

  // Khởi tạo từ query parameter (nếu có click từ trang Quản lý điểm đo)
  useEffect(() => {
    if (queryPointId) {
      const pId = Number(queryPointId);
      if (!isNaN(pId) && pId > 0) {
        setSelectedRecord({ measurementPointId: pId });
        setModalMode('create');
        setModalVisible(true);
      }
    }
  }, [queryPointId]);

  // Hàm tải dữ liệu biên bản treo tháo
  const fetchData = (page = currentPage, size = pageSize) => {
    const payload: any = {
      currentPage: page,
      pageSize: size,
      pointCode: debouncePointCode?.trim() || undefined,
      customerName: debounceCustomerName?.trim() || undefined,
      status: dataForm?.status || undefined,
    };

    getList(
      payload,
      (res: any) => {
        const responseData = res?.data;
        if (responseData) {
          setDataList(responseData.data || []);
          setTotal(responseData.total || 0);
          setCurrentPage(responseData.currentPage || 1);
          setPageSize(responseData.pageSize || 10);
        }
      },
      () => {
        notifyError('', 'Không thể tải danh sách biên bản treo tháo.');
      },
      setLoading
    );
  };

  // Tự động load dữ liệu khi bộ lọc hoặc phân trang thay đổi
  useEffect(() => {
    fetchData(1, pageSize);
  }, [debouncePointCode, debounceCustomerName, dataForm?.status, pageSize]);

  // Xóa nhanh toàn bộ bộ lọc
  const handleClearSearch = () => {
    setDataForm({
      pointCode: '',
      customerName: '',
      status: '',
    });
  };

  const handleOpenCreateModal = () => {
    setSelectedRecord(null);
    setModalMode('create');
    setModalVisible(true);
  };

  const handleOpenEditModal = (record: any) => {
    setSelectedRecord(record);
    setModalMode('edit');
    setModalVisible(true);
  };

  const handleOpenViewModal = (record: any) => {
    setSelectedRecord(record);
    setModalMode('view');
    setModalVisible(true);
  };

  const handleApprove = (record: any) => {
    approveReplacement(
      record.id,
      () => {
        notifySuccess('', `Đã phê duyệt biên bản #${record.id}!`);
        fetchData();
      },
      () => {
        notifyError('', 'Không thể phê duyệt biên bản.');
      },
      setLoading
    );
  };

  const handleCancel = (record: any) => {
    cancelReplacement(
      record.id,
      () => {
        notifySuccess('', `Đã hủy biên bản #${record.id}!`);
        fetchData();
      },
      () => {
        notifyError('', 'Không thể hủy biên bản.');
      },
      setLoading
    );
  };

  const handleDelete = (record: any) => {
    deleteReplacement(
      record.id,
      () => {
        notifySuccess('', `Đã xóa biên bản #${record.id}!`);
        fetchData();
      },
      () => {
        notifyError('', 'Không thể xóa biên bản.');
      },
      setLoading
    );
  };

  // Cấu hình các cột hiển thị trên 1 dòng duy nhất, kèm Tooltips khi tràn nội dung
  const columns = [
    {
      title: 'STT',
      key: 'stt',
      width: 55,
      align: 'center' as const,
      render: (_: any, __: any, index: number) => (
        <span className="whitespace-nowrap font-medium text-slate-600">
          {(currentPage - 1) * pageSize + index + 1}
        </span>
      ),
    },
    {
      title: 'Mã điểm đo',
      dataIndex: 'measurementPointCode',
      key: 'measurementPointCode',
      width: 150,
      render: (val: string, record: any) => {
        const fullInfo = `${val || '---'}${record.measurementPointDescription ? ` - ${record.measurementPointDescription}` : ''}`;
        return (
          <Tooltip title={fullInfo}>
            <div className="truncate font-semibold text-blue-700 max-w-[140px] cursor-pointer">
              {val || '---'}
            </div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Tên khách hàng',
      dataIndex: 'customerName',
      key: 'customerName',
      width: 200,
      render: (val: string, record: any) => {
        const fullCustomer = record.customerCode ? `[${record.customerCode}] ${val}` : (val || '---');
        return (
          <Tooltip title={fullCustomer}>
            <div className="truncate text-slate-800 max-w-[190px]">
              {val || '---'}
            </div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Thời điểm treo/tháo',
      dataIndex: 'replacementDate',
      key: 'replacementDate',
      width: 150,
      align: 'center' as const,
      render: (val: string) => (
        <span className="whitespace-nowrap font-medium text-slate-700">
          {val ? dayjs(val).format('DD/MM/YYYY HH:mm') : '---'}
        </span>
      ),
    },
    {
      title: 'Công tơ tháo',
      key: 'oldMeter',
      width: 170,
      render: (record: any) => {
        const summary = `${record.oldMeterSerial || '---'} (HSN: ${record.oldMeterMultiplier ?? 1})`;
        const details = (
          <div className="text-xs space-y-1 p-0.5">
            <div><b>Số công tơ tháo:</b> {record.oldMeterSerial || '---'}</div>
            <div><b>Hệ số nhân (HSN):</b> {record.oldMeterMultiplier ?? 1}</div>
            <div><b>Chỉ số BT:</b> {Number(record.oldBt || 0).toLocaleString()} kWh</div>
            <div><b>Chỉ số CD:</b> {Number(record.oldCd || 0).toLocaleString()} kWh</div>
            <div><b>Chỉ số TD:</b> {Number(record.oldTd || 0).toLocaleString()} kWh</div>
            <div><b>Chỉ số SG:</b> {Number(record.oldSg || 0).toLocaleString()} kWh</div>
            <div><b>Chỉ số VC:</b> {Number(record.oldVc || 0).toLocaleString()} kvarh</div>
            <div><b>Pmax:</b> {Number(record.oldPmax || 0).toLocaleString()} kW</div>
          </div>
        );
        return (
          <Tooltip title={details}>
            <div className="truncate font-mono text-amber-900 bg-amber-50 px-2 py-0.5 rounded border border-amber-200 text-xs max-w-[160px] cursor-pointer">
              {summary}
            </div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Công tơ treo',
      key: 'newMeter',
      width: 170,
      render: (record: any) => {
        const summary = `${record.newMeterSerial || '---'} (HSN: ${record.newMeterMultiplier ?? 1})`;
        const details = (
          <div className="text-xs space-y-1 p-0.5">
            <div><b>Số công tơ treo:</b> {record.newMeterSerial || '---'}</div>
            <div><b>Hệ số nhân (HSN):</b> {record.newMeterMultiplier ?? 1}</div>
            <div><b>Chỉ số BT:</b> {Number(record.newBt || 0).toLocaleString()} kWh</div>
            <div><b>Chỉ số CD:</b> {Number(record.newCd || 0).toLocaleString()} kWh</div>
            <div><b>Chỉ số TD:</b> {Number(record.newTd || 0).toLocaleString()} kWh</div>
            <div><b>Chỉ số SG:</b> {Number(record.newSg || 0).toLocaleString()} kWh</div>
            <div><b>Chỉ số VC:</b> {Number(record.newVc || 0).toLocaleString()} kvarh</div>
            <div><b>Pmax:</b> {Number(record.newPmax || 0).toLocaleString()} kW</div>
          </div>
        );
        return (
          <Tooltip title={details}>
            <div className="truncate font-mono text-emerald-900 bg-emerald-50 px-2 py-0.5 rounded border border-emerald-200 text-xs max-w-[160px] cursor-pointer">
              {summary}
            </div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Lý do thay thế',
      dataIndex: 'reason',
      key: 'reason',
      width: 180,
      render: (val: string) => (
        <Tooltip title={val || '---'}>
          <div className="truncate text-slate-700 max-w-[170px]">
            {val || '---'}
          </div>
        </Tooltip>
      ),
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      width: 130,
      align: 'center' as const,
      render: (status: string) => {
        if (status === 'APPROVED') {
          return <Tag color="success" className="whitespace-nowrap m-0">Đã phê duyệt</Tag>;
        }
        if (status === 'DRAFT') {
          return <Tag color="warning" className="whitespace-nowrap m-0">Bản nháp</Tag>;
        }
        return <Tag color="default" className="whitespace-nowrap m-0">Đã hủy</Tag>;
      },
    },
    {
      title: 'Thao tác',
      key: 'actions',
      width: 120,
      align: 'center' as const,
      fixed: 'right' as const,
      render: (record: any) => (
        <Space size="small" className="whitespace-nowrap">
          <Tooltip title="Xem chi tiết">
            <Button
              type="text"
              size="small"
              icon={<EyeOutlined className="text-blue-600" />}
              onClick={() => handleOpenViewModal(record)}
            />
          </Tooltip>

          {record.status === 'DRAFT' && (
            <>
              <Tooltip title="Phê duyệt">
                <Popconfirm
                  title="Phê duyệt biên bản treo tháo?"
                  description="Khi phê duyệt, hệ thống sẽ tự động cập nhật hệ số nhân mới vào Điểm đo."
                  onConfirm={() => handleApprove(record)}
                  okText="Phê duyệt"
                  cancelText="Hủy"
                >
                  <Button
                    type="text"
                    size="small"
                    icon={<CheckCircleOutlined className="text-emerald-600 font-bold" />}
                  />
                </Popconfirm>
              </Tooltip>

              <Tooltip title="Chỉnh sửa">
                <Button
                  type="text"
                  size="small"
                  icon={<EditOutlined className="text-amber-600" />}
                  onClick={() => handleOpenEditModal(record)}
                />
              </Tooltip>

              <Tooltip title="Xóa biên bản">
                <Popconfirm
                  title="Xóa vĩnh viễn biên bản nháp này?"
                  onConfirm={() => handleDelete(record)}
                  okText="Xóa"
                  cancelText="Không"
                >
                  <Button
                    type="text"
                    size="small"
                    icon={<DeleteOutlined className="text-red-500" />}
                  />
                </Popconfirm>
              </Tooltip>
            </>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div>
      <PageTitle title={'Quản lý treo tháo'} />
      <div className="flex justify-between items-center">
        <h1 className="text-24-28 font-bold">{_t('Quản lý treo tháo')}</h1>
        <div className="flex gap-1">
          <PrivateComponent roles={[Constants.Role.SuperAdmin.value, Constants.Role.Admin.value]}>
            <ButtonCommonCustomizable
              title={_t('')}
              tooltipText={'Lập biên bản treo tháo'}
              classButton="text-button-variant"
              onClick={handleOpenCreateModal}
              icon={<PlusOutlined size={20} />}
            />
          </PrivateComponent>
        </div>
      </div>

      <Card className="mt-5 shadow-custom">
        {/* THANH BỘ LỌC (MÃ ĐIỂM ĐO, TÊN KHÁCH HÀNG, TRẠNG THÁI) */}
        <div className="mb-5 flex justify-between items-center">
          <div className="flex items-center w-full gap-2">
            <div className="flex items-center gap-3 flex-1">
              {/* 1. Lọc theo Mã điểm đo */}
              <div className="w-60">
                <InputTextCommon
                  label={null}
                  attribute={'pointCode'}
                  dataAttribute={dataForm?.pointCode}
                  placeholder={_t('Nhập mã điểm đo')}
                  setData={setDataForm}
                  isRequired={false}
                  prefix={<IoSearch />}
                  allowClear={true}
                />
              </div>

              {/* 2. Lọc theo Tên khách hàng */}
              <div className="w-72">
                <InputTextCommon
                  label={null}
                  attribute={'customerName'}
                  dataAttribute={dataForm?.customerName}
                  placeholder={_t('Nhập tên khách hàng')}
                  setData={setDataForm}
                  isRequired={false}
                  prefix={<IoSearch />}
                  allowClear={true}
                />
              </div>

              {/* 3. Lọc theo Trạng thái */}
              <div className="w-56">
                <InputSelectCommon
                  label={null}
                  attribute={'status'}
                  dataAttribute={dataForm?.status}
                  placeholder={_t('Trạng thái')}
                  setData={setDataForm}
                  isRequired={false}
                  allowClear={true}
                  listDataItem={[
                    { label: 'Đã phê duyệt', value: 'APPROVED' },
                    { label: 'Bản nháp', value: 'DRAFT' },
                  ]}
                />
              </div>
            </div>

            {/* Nút Xóa nhanh bộ lọc */}
            <div className="flex gap-1">
              {dataForm?.pointCode || dataForm?.customerName || dataForm?.status ? (
                <ButtonCommonCustomizable
                  title={_t('')}
                  tooltipText="Xoá tất cả bộ lọc"
                  icon={<IoBackspaceOutline size={25} />}
                  classButton="text-button-variant"
                  onClick={handleClearSearch}
                  variant={'text'}
                />
              ) : null}
            </div>
          </div>
        </div>

        {/* BẢNG DỮ LIỆU ĐỒNG BỘ 1 DÒNG VỚI TOOLTIPS */}
        <div>
          <TableCommon
            columns={columns}
            dataSource={dataList}
            rowKey="id"
            loading={loading}
            pagination={false}
            scroll={{ x: 'max-content', y: window.innerHeight * 0.66 }}
          />

          {/* PHÂN TRANG */}
          <div className="flex justify-between items-center mt-4">
            <span className="text-sm text-slate-500">
              Tổng cộng: <b>{total}</b> biên bản
            </span>
            <Pagination
              current={currentPage}
              pageSize={pageSize}
              total={total}
              showSizeChanger
              pageSizeOptions={['10', '20', '50', '100']}
              onChange={(page, size) => {
                setCurrentPage(page);
                setPageSize(size);
                fetchData(page, size);
              }}
            />
          </div>
        </div>
      </Card>

      {/* MODAL LẬP / SỬA / XEM BIÊN BẢN */}
      <ReplacementModal
        visible={modalVisible}
        mode={modalMode}
        initialData={selectedRecord}
        onClose={() => setModalVisible(false)}
        onSuccess={() => fetchData()}
      />
    </div>
  );
};

export default MeterReplacementPage;
