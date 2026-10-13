import { SVGProps } from 'react';

export const IconFluctuation = (props: SVGProps<SVGSVGElement>) => (
  <svg xmlns='http://www.w3.org/2000/svg' width={56} height={57} fill='none' {...props}>
    <g filter='url(#a)'>
      <rect width={45} height={45} x={5.5} y={2.5} fill='#B22222' rx={12} />
      <g clipPath='url(#b)'>
        <path
          stroke='#fff'
          strokeLinecap='round'
          strokeLinejoin='round'
          strokeWidth={2}
          d='m37.167 20-7.917 7.917-4.166-4.167-6.25 6.25m18.333-10h-5m5 0v5'
        />
      </g>
    </g>
    <defs>
      <clipPath id='b'>
        <path fill='#fff' d='M18 15h20v20H18z' />
      </clipPath>
      <filter
        id='a'
        width={56}
        height={56}
        x={0}
        y={0.5}
        colorInterpolationFilters='sRGB'
        filterUnits='userSpaceOnUse'
      >
        <feFlood floodOpacity={0} result='BackgroundImageFix' />
        <feColorMatrix
          in='SourceAlpha'
          result='hardAlpha'
          values='0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0'
        />
        <feOffset dy={3.5} />
        <feGaussianBlur stdDeviation={2.75} />
        <feColorMatrix values='0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.02 0' />
        <feBlend in2='BackgroundImageFix' result='effect1_dropShadow_645_7813' />
        <feBlend in='SourceGraphic' in2='effect1_dropShadow_645_7813' result='shape' />
      </filter>
    </defs>
  </svg>
);
