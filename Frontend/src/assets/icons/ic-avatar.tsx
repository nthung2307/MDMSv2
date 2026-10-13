import { SVGProps } from 'react';

export const IconAvatar = (props: SVGProps<SVGSVGElement>) => (
  <svg xmlns='http://www.w3.org/2000/svg' width={56} height={57} fill='none' {...props}>
    <g filter='url(#a)'>
      <rect width={45} height={45} x={5.5} y={2.5} fill='#B22222' rx={12} />
      <path
        fill='#fff'
        d='M31.593 16.027C30.68 15.042 29.405 14.5 28 14.5c-1.414 0-2.692.539-3.6 1.517-.918.99-1.365 2.335-1.26 3.787C23.348 22.669 25.528 25 28 25c2.472 0 4.648-2.33 4.86-5.195.106-1.44-.344-2.781-1.267-3.778ZM36.25 35.499h-16.5a1.454 1.454 0 0 1-1.135-.522 1.655 1.655 0 0 1-.336-1.365c.395-2.194 1.63-4.038 3.57-5.332 1.725-1.15 3.909-1.781 6.15-1.781 2.243 0 4.427.632 6.15 1.78 1.941 1.295 3.176 3.138 3.572 5.333.09.503-.032 1-.337 1.364a1.453 1.453 0 0 1-1.134.523Z'
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
        <feBlend in2='BackgroundImageFix' result='effect1_dropShadow_645_7801' />
        <feBlend in='SourceGraphic' in2='effect1_dropShadow_645_7801' result='shape' />
      </filter>
    </defs>
  </svg>
);
