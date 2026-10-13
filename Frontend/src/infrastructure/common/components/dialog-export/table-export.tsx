import { Text, View, Image } from '@react-pdf/renderer';
import { StyleSheet } from '@react-pdf/renderer';

import EditedIc from '@/assets/icons/edited-ic.png';

interface Props {
  dataSource: any;
  listHeader: any;
}

const TableExport = (props: Props) => {
  const { dataSource, listHeader } = props;
  return (
    <View style={[styles.table]}>
      {/* Header */}
      <View style={[styles.tableRow, styles.tableHeader]} fixed>
        {listHeader.map((item: any, index: number) => (
          <Text
            key={index}
            style={[
              styles.tableCusCell,
              styles.tableCellHightLight,
              index == 0 ? styles.tableCellHeaderStart : styles.tableCellHeader,
              { width: `${item?.width || 5}%` },
            ]}
          >
            {item.label}
          </Text>
        ))}
      </View>

      {/* Data Rows */}
      {dataSource.map((item: any, rowIndex: number) => (
        <View key={rowIndex} style={styles.tableRow} wrap={false}>
          {listHeader.map((it: any, i: number) => {
            if (it.attr == 'status' && it.activeMsg)
              return (
                <View
                  key={i}
                  style={[
                    styles.tableCusCell,
                    i == 0 ? styles.tableCellStart : styles.tableCell,
                    { width: `${it?.width || 5}%` },
                  ]}
                >
                  <View
                    style={[
                      styles.tableCellStatus,
                      {
                        color: item[it.attr] == 'ACTIVE' ? '#41a026' : '#b22222',
                        fontWeight: 'ultrabold',
                      },
                    ]}
                  >
                    <Text>{item[it.attr] == 'ACTIVE' ? it.activeMsg : it.inactiveMsg}</Text>
                  </View>
                </View>
              );
            return (
              <Text
                key={i}
                style={[
                  styles.tableCusCell,
                  i == 0 ? styles.tableCellStart : styles.tableCell,
                  { width: `${it?.width || 5}%` },
                ]}
              >
                <Text>{item[it.attr]} </Text>
                {item.isChange && it.attr == 'currentReading' && (
                  <Image src={EditedIc} style={styles.image} />
                )}
              </Text>
            );
          })}
        </View>
      ))}
    </View>
  );
};

export default TableExport;

const styles = StyleSheet.create({
  table: {
    display: 'flex',
    width: '100%',
    marginBottom: 15,
  },
  tableRow: {
    flexDirection: 'row',
  },
  tableHeader: {
    fontWeight: 'bold',
  },

  tableCellHightLight: {
    backgroundColor: '#d3d3d3ca',
  },

  tableCusCell: {
    fontSize: 11,
    borderStyle: 'solid',
    borderColor: '#000',
    padding: 5,
    textAlign: 'left',
  },
  tableCellHeader: {
    borderRightWidth: 2,
    borderTopWidth: 2,
    borderBottomWidth: 2,
  },
  tableCellHeaderStart: {
    borderWidth: 2,
  },
  tableCellStart: {
    borderBottomWidth: 2,
    borderRightWidth: 2,
    borderLeftWidth: 2,
  },
  tableCell: {
    borderBottomWidth: 2,
    borderRightWidth: 2,
  },

  tableCellEnd: {
    borderBottomWidth: 2,
    borderRightWidth: 2,
  },

  tableCellStatus: {
    color: '#fff',
    textAlign: 'center',
    borderRadius: 15,
    paddingTop: 2,
    paddingBottom: 2,
  },

  image: {
    width: 20,
    height: 12,
    paddingRight: 6,
  },
});
