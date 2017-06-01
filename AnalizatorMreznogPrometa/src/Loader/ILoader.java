package Loader;

import java.util.LinkedList;

public interface ILoader {
	public int GetKolicinaTestnihPodataka();

	public LinkedList<PrometData> Load(String[] onlyLoad);
}
