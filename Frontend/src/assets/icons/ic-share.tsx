import { SVGProps } from 'react';

export const IconShare = (props: SVGProps<SVGSVGElement>) => (
  <svg xmlns='http://www.w3.org/2000/svg' width={56} height={57} fill='none' {...props}>
    <g filter='url(#a)'>
      <rect width={45} height={45} x={5.5} y={2.5} fill='#B22222' rx={12} />
      <path
        stroke='#fff'
        strokeLinecap='round'
        strokeLinejoin='round'
        strokeWidth={2.5}
        d='m24.59 26.51 6.83 3.98m-.01-10.98-6.82 3.98M37 18a3 3 0 1 1-6 0 3 3 0 0 1 6 0Zm-12 7a3 3 0 1 1-6 0 3 3 0 0 1 6 0Zm12 7a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z'
      />
    </g>
    <defs>
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
        <feBlend in2='BackgroundImageFix' result='effect1_dropShadow_645_7830' />
        <feBlend in='SourceGraphic' in2='effect1_dropShadow_645_7830' result='shape' />
      </filter>
    </defs>
  </svg>
);
