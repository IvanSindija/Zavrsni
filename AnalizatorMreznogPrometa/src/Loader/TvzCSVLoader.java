package Loader;

import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;
import java.util.LinkedList;

public class TvzCSVLoader implements ILoader {
	private int n;
	private String fileName;
	public int KolicinaTestnihPodataka = 0;

	public TvzCSVLoader(String fileName, int n) {
		this.fileName = System.getProperty("user.dir") + fileName;
		this.n = n;
	}

	@Override
	public LinkedList<PrometData> Load(String[] onlyLoad) {
		LinkedList<PrometData> promet = new LinkedList<PrometData>();
		int validanPromet = 0;
		int validanOdgovor = 0;
		int falsePromet = 0;
		try (BufferedReader br = new BufferedReader(new FileReader(fileName))) {
			String line;

			while ((line = br.readLine()) != null) {
				validanPromet++;
				if (validanPromet >= n) {
					break;
				}
				// System.out.println(line);
				String[] parsedLine = line.split(";");
				PrometData linijaPrometa = new PrometData();
				// System.out.print(parsedLine[0]);
				// System.out.print(parsedLine[1]);
				// System.out.print(parsedLine[2]);
				// System.out.print(parsedLine[3]);
				// System.out.print(parsedLine[4]);
				// System.out.print(parsedLine[5]);
				// System.out.print(parsedLine[6]);
				// System.out.print("\n");
				boolean loadOk = false;
				for (String vrsteLoad : onlyLoad) {
					if (vrsteLoad.equals(parsedLine[8])) {
						loadOk = true;
					}
				}

				if (loadOk) {
					linijaPrometa.Time = parsedLine[3];
					linijaPrometa.Vrsta = parsedLine[8];
					linijaPrometa.SEQ = parsedLine[9];
					linijaPrometa.ASEQ = Long.parseLong(parsedLine[10]);
					linijaPrometa.SourceIP = parsedLine[12];
					linijaPrometa.SourcePort = parsedLine[13];
					linijaPrometa.DestinationIP = parsedLine[14];
					linijaPrometa.DestinationPort = parsedLine[15];
					//KolicinaTestnihPodataka je za procjenu false positive, a to
					//se gleda broj gresaka/broj ACK(*) zahtjeva
					if(!linijaPrometa.Vrsta.equals("SYN-ACK")){
						KolicinaTestnihPodataka++;
					}
					// if(validanPromet<n){
					promet.addLast(linijaPrometa);
					// validanPromet++;
					// }
					// }else if(linijaPrometa.Vrsta.equals("ACK(SYN)")){
					// boolean pronaden = false;
					// for (PrometData zahtjevIzListe : promet) {
					/*
					 * if (zahtjevIzListe.SourceIP
					 * .equals(linijaPrometa.DestinationIP) &&
					 * zahtjevIzListe.SourcePort
					 * .equals(linijaPrometa.DestinationPort) &&
					 * zahtjevIzListe.DestinationIP
					 * .equals(linijaPrometa.SourceIP) &&
					 * zahtjevIzListe.DestinationPort
					 * .equals(linijaPrometa.SourcePort) && zahtjevIzListe.ASEQ
					 * == Long .parseLong(linijaPrometa.SEQ) && Long
					 * .parseLong(zahtjevIzListe.SEQ) == (linijaPrometa.ASEQ -
					 * 1)) { pronaden=true; } } if(!pronaden){ //
					 * System.out.println("Desilo se nesto lose !"+line); }
					 * if(pronaden){ promet.addLast(linijaPrometa);
					 * validanOdgovor++; } }else{ promet.addLast(linijaPrometa);
					 * falsePromet++; } } if(validanOdgovor==n){
					 * System.out.println("svi odgovori popunjeni sve OK");
					 * break; }
					 */
					}
			}
		} catch (IOException e) {
			e.printStackTrace();
		}
		
		System.out.println("KolicinaTestnihPodataka=" + KolicinaTestnihPodataka);
		return promet;
	}

	@Override
	public int GetKolicinaTestnihPodataka() {
		return KolicinaTestnihPodataka;
	}

}
