/*package Loader;

import java.util.LinkedList;

import org.apache.poi.ss.usermodel.Cell;
import org.apache.poi.ss.usermodel.Row;
import org.apache.poi.ss.usermodel.Sheet;
import org.apache.poi.ss.usermodel.Workbook;
import org.apache.poi.ss.usermodel.WorkbookFactory;
import java.io.File;

public class ExelLoader implements ILoader {

	private String fileName;

	public ExelLoader(String fileName) {
		this.fileName = System.getProperty("user.dir") + fileName;
		System.out.println(this.fileName);
	}

	@Override
	public LinkedList<PrometData> Load(String[] onlyLoad) {
		LinkedList<PrometData> promet = new LinkedList<PrometData>();
		try {

			File f = new File(fileName);
			Workbook wb = WorkbookFactory.create(f);
			Sheet mySheet = wb.getSheetAt(0);
			// Iterator<Row> rowIter = mySheet.rowIterator();

			for (Row redTablice : mySheet) {
				Cell vrsta = redTablice.getCell(8);
				Cell seq = redTablice.getCell(9);
				Cell ack = redTablice.getCell(10);
				Cell ipOd = redTablice.getCell(12);
				Cell portOd = redTablice.getCell(13);
				Cell ipZa = redTablice.getCell(14);
				Cell portZa = redTablice.getCell(15);
				try {
					PrometData zahtjev = new PrometData();
					boolean ucitajZahtjev = false;
					for(String x:onlyLoad){
						if(x.equals(vrsta.getStringCellValue())){
							ucitajZahtjev=true;
							break;
						}
					}
					if(ucitajZahtjev){
						zahtjev.Vrsta=vrsta.toString();
						zahtjev.SEQ=seq.toString();
						zahtjev.ASEQ=ack.getNumericCellValue();
						zahtjev.SourceIP=ipOd.toString();
						zahtjev.SorcePort=portOd.toString();
						zahtjev.DestinationIP=ipZa.toString();
						zahtjev.DestinationPort=portZa.toString();
						
						promet.addLast(zahtjev);
					}
					
				} catch (Exception e) {
					System.out.println("los format");
				}
			}
		} catch (Exception e) {
			System.out.println("exception");
			// e.printStackTrace();
		}
		return promet;
	}

}
*/