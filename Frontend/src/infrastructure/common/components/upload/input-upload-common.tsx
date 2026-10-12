import { Col, Row, Upload, UploadProps } from 'antd';
import { useEffect, useState } from 'react';

import iconUploadImage from '@/assets/icons/ic_upload_image.svg';
import { FileType } from '@/core/domain/enums/FileType';
import { notifyError } from '@/infrastructure/common/components/toast/toastMessage';
import { _t } from '@/infrastructure/utils/translation';

const getBase64 = (img: FileType, callback: (url: string) => void) => {
  const reader = new FileReader();
  reader.addEventListener('load', () => callback(reader.result as string));
  const blob = new Blob([img]);
  reader.readAsDataURL(blob);
};

interface InputUploadCommonProps {
  onUploadAsync: (file: any) => Promise<void>;
  dataAttribute: string;
  label: string;
  isRequired?: boolean;
  disabled?: boolean;
  // imageUrl?: string;
  // setImageUrl: any;
}

const InputUploadCommon = (props: InputUploadCommonProps) => {
  const { onUploadAsync, dataAttribute, label, isRequired, disabled } = props;
  const baseURL = import.meta.env.VITE_APP_API_URL;
  const [, setLoading] = useState<boolean>(false);
  const [imageUrl, setImageUrl] = useState<any>();

  const handleChange: UploadProps['onChange'] = (info) => {
    if (disabled) {
      return;
    }
    getBase64(info.file.originFileObj as unknown as FileType, () => {
      setLoading(false);
    });
  };
  const beforeUpload = (file: any) => {
    const isJpgOrPng = file.type === 'image/jpeg' || file.type === 'image/png';
    if (!isJpgOrPng) {
      notifyError('', 'Định dạng ảnh không chính xác. Vui lòng thử lại');
    }
    const isLt1M = file.size / 1024 / 1024 < 1;
    if (!isLt1M) {
      notifyError('', 'Giới hạn dung lượng ảnh 1MB. Vui lòng thử lại');
    }
    return isJpgOrPng && isLt1M;
  };

  useEffect(() => {
    if (dataAttribute) {
      setImageUrl(dataAttribute);
    }
  }, [dataAttribute]);

  return (
    <Col className='gap-4'>
      <Row className='mb-1'>
        <Col className={'font-bold text-14-20 text-[#3B3B3B]'}>
          <label>{label}</label>
        </Col>
        <Col className='is-required'>
          {isRequired ? <span className='ml-1 text-red-ff0000'>*</span> : null}
        </Col>
      </Row>
      <Upload
        action={(file) => {
          return new Promise(() => {
            onUploadAsync(file).then(() => {});
          });
        }}
        name='avatar'
        className='avatar-uploader w-fulls'
        showUploadList={false}
        beforeUpload={beforeUpload}
        onChange={handleChange}
        disabled={disabled}
      >
        {imageUrl && dataAttribute ? (
          <img
            src={`${baseURL}/${imageUrl}`}
            className='min-h-[82px] max-h-[200px] w-full object-scale-down'
            alt='avatar'
            style={{ width: '100%' }}
          />
        ) : (
          <div className='flex flex-col items-center justify-center rounded-[5px] border border-dashed border-[#C4C4C4] cursor-pointer min-h-[82px] text-12-18 font-bold'>
            <img src={iconUploadImage} alt={''} className={`w-6 h-6 mb-[10px]`} />
            {_t('Tải ảnh lên')}
          </div>
        )}
      </Upload>
    </Col>
  );
};

export default InputUploadCommon;
