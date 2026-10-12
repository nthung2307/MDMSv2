import { Upload, UploadProps } from 'antd';
import { useEffect, useState } from 'react';

import iconAvatar from '@/assets/icons/ic_avatar.svg';
import { FileType } from '@/core/domain/enums/FileType';
import { ButtonStyled } from '@/infrastructure/common/components/button/button-styled';
import { notifyError } from '@/infrastructure/common/components/toast/toastMessage';

const getBase64 = (img: FileType, callback: (url: string) => void) => {
  const reader = new FileReader();
  reader.addEventListener('load', () => callback(reader.result as string));
  const blob = new Blob([img]);
  reader.readAsDataURL(blob);
};

interface UploadAvatarCommonProps {
  onUploadAsync: (file: any) => Promise<void>;
  dataAttribute?: any;
}

const UploadAvatarCommon = (props: UploadAvatarCommonProps) => {
  const { onUploadAsync, dataAttribute } = props;
  const baseURL = import.meta.env.VITE_APP_API_URL;
  const [imageUrl, setImageUrl] = useState<string | null>(null);
  const [, setLoading] = useState<boolean>(false);

  const handleChange: UploadProps['onChange'] = (info) => {
    getBase64(info.file.originFileObj as unknown as FileType, () => {
      setLoading(false);
    });
  };
  const beforeUpload = (file: any) => {
    const isJpgOrPng =
      file.type === 'image/jpeg' ||
      file.type === 'image/png' ||
      file.type === 'image/jpg' ||
      file.type === 'image/heic';
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
    <div className='flex flex-col items-center gap-4'>
      <div className='w-28 h-28'>
        {imageUrl ? (
          <img
            src={`${baseURL}/${imageUrl}`}
            className='upload-avatar-image'
            alt='avatar'
            style={{ width: '100%' }}
          />
        ) : (
          <img
            src={iconAvatar}
            className='upload-avatar-image'
            alt='avatar'
            style={{ width: '100%' }}
          />
        )}
      </div>
      <p className='text-12-12 text-gray-909090'>JPG, JPEG, PNG, JFIF, HEIC</p>

      <Upload
        action={(file) => {
          return new Promise(() => {
            onUploadAsync(file).then(() => {});
          });
        }}
        name='avatar'
        accept='.jpg,.jpeg,.png,.jfif,.heic'
        className='avatar-uploader'
        showUploadList={false}
        beforeUpload={beforeUpload}
        onChange={handleChange}
      >
        <ButtonStyled
          borderRadius='50px'
          bg='white'
          border='1px solid #006CF1'
          color='#006CF1'
          minHeight='45px'
          fontSize='14px'
          minWidth='150px'
        >
          Upload ảnh
        </ButtonStyled>
      </Upload>
    </div>
  );
};

export default UploadAvatarCommon;
