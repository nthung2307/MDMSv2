import { Card } from 'antd';
import { useEffect, useRef, useState } from 'react';
import { FiDownload, FiShare } from 'react-icons/fi';
import { LuSave } from 'react-icons/lu';
import { useLocation, useParams } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { tagNameListRequest } from '@/core/application/dto/tag-name/requests/TagNameListRequest';
import { tagNameRequest } from '@/core/application/dto/tag-name/requests/TagNameRequest';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifyError, notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import useCreateTagNameHook from '@/infrastructure/repository/config/hooks/useCreateTagName.hook';
import useGetTagNameListHook from '@/infrastructure/repository/config/hooks/useGetListTagName.hook';
import { _t } from '@/infrastructure/utils/translation';
import { downloadExcelTemplate } from '@/infrastructure/utils/excelHelper';
import DialogImportConfig from '@/pages/config/components/dialog-import-config';
import TableTagNameManagement from '@/pages/config/components/table-tag-name';

const DataMeasurementPoint = () => {
  const { id } = useParams();
  const location = useLocation();
  const searchParams = new URLSearchParams(location.search);
  const queryCode = searchParams.get('code');
  const record = location.state?.record || { code: queryCode || '' };

  const [getTagNameList] = useGetTagNameListHook();
  const [createTagNameList] = useCreateTagNameHook();
  const [loading, setLoading] = useState<boolean>(false);
  const [listTagName, setListTagName] = useState<any>();
  const [typeTable, setTypeTable] = useState<any>(Constants.TYPE_TABLE.operatingParams.value);
  const [isShowImport, setIsShowImport] = useState<boolean>(false);

  const getTagNameListAsync = async () => {
    const params: tagNameListRequest = {
      measurementPoint: Number(id),
    };
    await getTagNameList(
      params,
      (res: DataResponse) => {
        const { data } = res;
        const dataUpdate = Constants.dataMeasurementPoint.allDataParams.map((param) => {
          const nowData = data.find((item: any) => item.description == param.value);
          const fixedUnit = param.unit && param.unit.length === 1 ? param.unit[0].value : null;
          const assignedUnit =
            fixedUnit ||
            (nowData?.unit
              ? nowData.unit
              : param.unit && param.unit.length > 0
                ? param.unit[0].value
                : '');

          return {
            description: param.value,
            tagName: nowData?.tagName || '',
            unit: assignedUnit,
            customName: nowData?.customName || '',
            type: param.type,
          };
        });
        setListTagName(dataUpdate);
      },
      () => {},
      setLoading
    );
  };

  const createTagNameAsync = async () => {
    const params: tagNameRequest = {
      tagNames: listTagName,
      measurementPointId: Number(id),
    };
    await createTagNameList(
      params,
      () => {
        notifySuccess('', _t('Cập nhật dữ liệu thành công'));
      },
      () => {},
      setLoading
    );
  };

  const handleExportCsv = async () => {
    if (!listTagName || listTagName.length === 0) {
      notifyError('', 'Chưa có dữ liệu để xuất');
      return;
    }

    const getGroupName = (type: string) => {
      switch (type) {
        case 'ELECTRICAL_PARAMETERS':
          return 'Thông số vận hành';
        case 'OPERATING_PARAMETERS':
          return 'Thông số điện năng';
        case 'WARNING':
          return 'Cảnh báo';
        case 'TIME':
        case 'DATETIME':
          return 'Thời gian';
        default:
          return 'Khác';
      }
    };

    try {
      const rows = Constants.dataMeasurementPoint.allDataParams.map((param: any, idx: number) => {
        const item = listTagName.find((t: any) => t.description === param.value) || {};
        return {
          stt: idx + 1,
          groupName: getGroupName(param.type),
          label: param.label || '',
          code: param.value || '',
          tagId: item.tagName || '',
          unit: item.unit || (param.unit && param.unit.length > 0 ? param.unit[0].value : ''),
          customName: item.customName || '',
        };
      });

      const pointCode = record?.code || id;
      await downloadExcelTemplate(
        `LienKetDuLieu_${pointCode}.xlsx`,
        'LienKetDuLieu',
        [
          { header: 'STT', key: 'stt', width: 8 },
          { header: 'Nhóm thông số', key: 'groupName', width: 22 },
          { header: 'Tên thông số', key: 'label', width: 26 },
          { header: 'Mã tham số', key: 'code', width: 20 },
          { header: 'Tag ID', key: 'tagId', width: 26 },
          { header: 'Đơn vị', key: 'unit', width: 14 },
          { header: 'Tên cảnh báo', key: 'customName', width: 26 },
        ],
        rows
      );
      notifySuccess('', _t('Xuất file Excel thành công'));
    } catch (err: any) {
      notifyError(err, 'Lỗi khi xuất file Excel');
    }
  };

  const validateWarning = listTagName
    ? listTagName.find(
        (item: any) =>
          item.description !== 'MAT_KET_NOI' &&
          item.type === 'WARNING' &&
          ((item.tagName && !item.customName) || (!item.tagName && item.customName))
      )
    : false;

  useEffect(() => {
    getTagNameListAsync();
  }, []);

  return (
    <div>
      <PageTitle title={'Cài đặt hệ thống'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Cài đặt hệ thống' },
            { name: 'Liên kết dữ liệu', link: ROUTE_PATH.CONFIGURATION },
            { name: record.code },
          ]}
        />
        <div className='flex gap-2 items-center'>
          <ButtonCommonCustomizable
            title={''}
            classButton='text-button-variant'
            tooltipText={'Xuất Excel'}
            onClick={handleExportCsv}
            icon={<FiShare size={20} />}
          />
          <ButtonCommonCustomizable
            title={''}
            classButton='text-button-variant'
            tooltipText={'Nhập từ Excel'}
            onClick={() => setIsShowImport(true)}
            icon={<FiDownload size={20} />}
          />
          <ButtonCommonCustomizable
            title={''}
            classButton='text-button-variant'
            tooltipText={'Lưu'}
            disabled={validateWarning}
            onClick={createTagNameAsync}
            icon={<LuSave size={20} />}
          />
        </div>
      </div>
      <Card className='mt-5'>
        <div className='flex border-b-slate-200 border-b-[1px] mb-4'>
          {Constants.TYPE_TABLE.typeArray.map((item, index) => (
            <button
              key={index}
              type='button'
              className={`px-4 py-3 cursor-pointer text-16-20 font-semibold ${item.value === typeTable && 'text-[#E98D8D] border-[#E98D8D] border-b-[1px]'}`}
              onClick={() => setTypeTable(item.value)}
            >
              <p>{item.label}</p>
            </button>
          ))}
        </div>
        <div className={'flex-1'}>
          <TableTagNameManagement
            listTagName={listTagName}
            setListTagName={setListTagName}
            typeTable={typeTable}
            loading={loading}
          />
        </div>
      </Card>
      <DialogImportConfig
        open={isShowImport}
        handleCancel={() => setIsShowImport(false)}
        onSuccess={(updatedList) => {
          setListTagName(updatedList);
        }}
        currentListTagName={listTagName || []}
        pointCode={record?.code || id || ''}
      />
    </div>
  );
};

export default DataMeasurementPoint;
