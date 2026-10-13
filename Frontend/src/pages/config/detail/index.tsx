import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { IoCopyOutline } from 'react-icons/io5';
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
import DialogDuplicate from '@/pages/config/components/duplicate-dialog';
import TableTagNameManagement from '@/pages/config/components/table-tag-name';
const DataMeasurementPoint = () => {
  const { id } = useParams();
  const { record } = useLocation().state;
  const [getTagNameList] = useGetTagNameListHook();
  const [createTagNameList] = useCreateTagNameHook();
  const [loading, setLoading] = useState<boolean>(false);
  const [dialogDuplicate, setDialogDuplicate] = useState<boolean>(false);
  const [listTagName, setListTagName] = useState<any>();
  const [typeTable, setTypeTable] = useState<any>(Constants.TYPE_TABLE.operatingParams.value);

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
          return {
            description: param.value,
            tagName: nowData?.tagName || '',
            unit: nowData?.unit ? nowData.unit : param.unit.length != 0 ? param.unit[0].value : '',
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

  const duplicateTagNameAsync = async (arr: number[]) => {
    const promises = arr.map((numberId) => {
      const params: tagNameRequest = {
        tagNames: listTagName,
        measurementPointId: numberId,
      };

      return new Promise<void>((resolve, reject) => {
        createTagNameList(
          params,
          () => {
            resolve();
          },
          () => {
            reject('error');
          },
          setLoading
        );
      });
    });

    try {
      await Promise.all(promises);
      notifySuccess('', 'Sao chép điểm đo thành công');
    } catch (error) {
      notifyError('', 'Sao chép điểm đo thất bại');
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
        <div className='flex gap-5'>
          <ButtonCommonCustomizable
            title={''}
            classButton='text-button-variant'
            tooltipText={'Sao chép'}
            disabled={validateWarning}
            onClick={() => {
              setDialogDuplicate(true);
            }}
            icon={<IoCopyOutline size={20} />}
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
      <DialogDuplicate
        open={dialogDuplicate}
        handleCancel={() => setDialogDuplicate(false)}
        handelSubmit={duplicateTagNameAsync}
        setGCSName={undefined}
      />
    </div>
  );
};

export default DataMeasurementPoint;
