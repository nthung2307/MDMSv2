import { RightOutlined } from '@ant-design/icons';
import { Breadcrumb } from 'antd';
import { Link } from 'react-router-dom';

import { _t } from '@/infrastructure/utils/translation';

interface IBreadcrumb {
  link?: string;
  name: string;
  state?: any;
}
type Props = {
  items: IBreadcrumb[];
};

export const BreadcrumbUI = (props: Props) => {
  const { items } = props;
  return (
    <div className={'flex flex-row gap-4 items-baseline'}>
      <Breadcrumb
        separator={<RightOutlined />}
        className={'font-bold text-18-18 breadcrumb-css'}
        items={items?.map((it) => {
          return it?.link
            ? {
                title: (
                  <Link to={it.link} state={it.state}>
                    {_t(it.name)}
                  </Link>
                ),
              }
            : { title: _t(it.name) };
        })}
      />
    </div>
  );
};
