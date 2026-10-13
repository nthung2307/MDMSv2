import { Document, Font, Page, Text, Image, View } from '@react-pdf/renderer';
import { StyleSheet } from '@react-pdf/renderer';
import dayjs from 'dayjs';
import React from 'react';

import Logo from '@/assets/logo-export.png';
import TableExport from '@/infrastructure/common/components/dialog-export/table-export';

type PdfProps = {
  dataSource?: any;
  listHeader: any;
  title: string;
  chartUrl?: any;
};

Font.register({
  family: 'Open Sans',
  fonts: [
    { src: 'https://cdn.jsdelivr.net/npm/open-sans-all@0.1.3/fonts/open-sans-regular.ttf' },
    {
      src: 'https://cdn.jsdelivr.net/npm/open-sans-all@0.1.3/fonts/open-sans-600.ttf',
      fontWeight: 1000,
    },
  ],
});
// Tạo nội dung PDF
const TemplateExport: React.FC<PdfProps> = ({ dataSource, listHeader, title, chartUrl }) => {
  const currentTime = dayjs().format('HH:mm:ss, DD/MM/YYYY');
  const chartHeight = chartUrl?.height ? (800 * chartUrl?.height) / chartUrl?.width : 0.75;

  const styles = StyleSheet.create({
    page: {
      padding: 15,
      fontSize: 12,
      fontFamily: 'Open Sans',
      flexDirection: 'column',
      justifyContent: 'space-between',
      height: '100%',
    },
    titleContainer: {
      textAlign: 'center',
      display: 'flex',
      flexDirection: 'row',
      justifyContent: 'space-between',
      width: '100%',
      fontSize: 11,
      fontWeight: 500,
      marginBottom: 10,
    },

    titleHeader: {
      textAlign: 'center',
      fontSize: 14,
      marginBottom: 10,
    },
    title: {
      fontSize: 16,
      fontWeight: 1000,
      marginBottom: 2,
      width: '100%',
      textAlign: 'center',
    },
    image: {
      width: 55,
      height: 17,
      paddingRight: 6,
    },

    // imageChart: { width: 800, height: chartHeight },
    imageChart: {
      width: '100%', // Scale chart to fit available width
      height: chartHeight, // Dynamically calculated height
      // maxHeight: 400, // Prevent chart from overflowing
      objectFit: 'contain', // Ensure chart fits within bounds
    },

    headerPage: {
      display: 'flex',
      flexDirection: 'row',
      justifyContent: 'flex-start',
      marginBottom: 10,
      alignItems: 'center',
    },

    time: {
      fontSize: 10,
    },

    footer: {
      // textAlign: 'left',
      display: 'flex',
      flexDirection: 'row',
      justifyContent: 'space-between',
      marginTop: 'auto',
      paddingTop: 10,
    },
    pageNumber: {
      fontSize: 10,
    },
  });
  return (
    <Document>
      <Page orientation='landscape' size='A4' style={styles.page}>
        {/* Phần tiêu đề */}
        <View style={styles.titleContainer} fixed>
          <View style={styles.headerPage}>
            <Image src={Logo} style={styles.image} />
            <Text>{' - Hệ thống quản lý dữ liệu điểm đo'}</Text>
          </View>
          <Text>{title}</Text>
        </View>

        <View style={styles.titleHeader}>
          <Text style={styles.title}>{title}</Text>
        </View>
        {/* Phần bảng */}
        {chartUrl ? (
          <View wrap={true}>
            <Image src={chartUrl.image} style={styles.imageChart} />
          </View>
        ) : (
          <TableExport dataSource={dataSource} listHeader={listHeader} />
        )}
        {/* Footer */}
        <View style={styles.footer} fixed>
          <Text style={styles.time}>Thời gian: {currentTime}</Text>
          <Text
            break
            style={styles.pageNumber}
            render={({ pageNumber, totalPages }) => `Trang ${pageNumber} / ${totalPages}`}
          />
        </View>
      </Page>
    </Document>
  );
};

export default TemplateExport;
