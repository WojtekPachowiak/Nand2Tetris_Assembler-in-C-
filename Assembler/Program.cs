using System;
using System.Collections.Generic;

namespace Assembler {
	class Program {

		static string path = @"C:\Users\48501\Desktop\Programming\C#\Assembler";
		static string path_Writes = @"\Writes";
		static string path_Reads = @"\Reads";
		static string[] file_names = { "Add", "Max", "MaxL", "Rect", "RectL", "Pong", "PongL" };



		static void Main(string[] args) {
			foreach (var p in file_names) {
				Assembler assembler = new Assembler();
				//Console.WriteLine("===============" + p + "=================");
				string[] text = System.IO.File.ReadAllLines(path + path_Reads + @"\" + p + @".asm");

				List<string> lineLst = new List<string>(text.Length);

				for (int i = 0, k=0; i < text.Length; i++) {
					string line = assembler.ProcessLineAndAddGOTOReferences(text[i], k);
					if (line != null) {
						lineLst.Add(line);
						k++;
					}
					
				}
				foreach (var line in lineLst) {
					assembler.AddVariablesSymbols(line);
				}

				//assembler.ViewSymbolTable();

				for (int i = 0; i < lineLst.Count; i++) {
					assembler.ParseLine(lineLst[i]);
					string machineCodeLine = assembler.GenerateMachineCode();

					//assembler.ViewInstr();
					//Console.WriteLine(lineLst[i]);
					//Console.WriteLine(machineCodeLine);
					//Console.WriteLine("++++++++++++++++++++++");
					lineLst[i] = machineCodeLine;
				}

				System.IO.File.WriteAllLines(path + path_Writes + @"\" + p + @".hack", lineLst);
				
			}
			
		}


	}
	public class Assembler {


		public Assembler() {
			for (int i = 0; i <= 15; i++) {
				symbolTable.Add("R" + i.ToString(), i);
				freeAddress = 16;
			}

		}

		int freeAddress = 16;

		struct Instr {
			public int type; //A or C instruction
			public int value;  //value of A instuction
			public int a;
			public string dest;
			public string comp;
			public string jump;
		}

		Instr instr = new Instr();


		Dictionary<string, int> symbolTable =
			new Dictionary<string, int>(){
							{"SCREEN", 16384},
							{"KBD", 24576},
							{"SP", 0},
							{"LCL", 1},
							{"ARG", 2},
							{"THIS", 3},
							{"THAT", 4}
							};
		


		public void ViewSymbolTable() {
			foreach (var item in symbolTable) {
				Console.WriteLine(item.Key + " : " + item.Value);
			}
		}
		public void ViewInstr(){
			Console.WriteLine("\ntype: " + instr.type.ToString() + "\n" +
								"================" + "\n" +
								"a: " + instr.a + "\n" +
								"dest: " + instr.dest + "\n" +
								"comp: " + instr.comp + "\n" +
								"jump: " + instr.jump + "\n" +
								"================" + "\n" +
								"value: " + instr.value + "\n");
		}

		public void ParseLine(string line) {
			if (line[0] == '@') {
				instr.type = 0;
				string substr = line.Substring(1);
				if (Char.IsDigit(substr[0]))
					instr.value = Int16.Parse(substr);
				else
					instr.value = symbolTable[substr];
			}
			else {
				instr.type = 1;
				//jump
				int i = line.IndexOf(';');
				if (i != -1)
					instr.jump = line.Substring(i + 1);
				else
					instr.jump = null;

				//dest
				int j = line.IndexOf('=');
				if (j != -1)
					instr.dest = line.Substring(0, j);
				else
					instr.dest = null;

				//comp
				if (j != -1) {
					if (i != -1)
						instr.comp = line.Substring(j + 1, i - j + 1);
					else
						instr.comp = line.Substring(j + 1);
				}
				else {
						instr.comp = line.Substring(0, i);
				}
				//a
				instr.a = instr.comp.Contains('M') ? 1 : 0 ;



			}
			
		}

		public string ProcessLineAndAddGOTOReferences(string line, int lineNum) {
			
				line = line.Replace(" ", "");

				if (line == "")
					return null;

				if (line.Substring(0, 2) == "//")
					return null;
				if (line[0] == '(') {
					int i = line.IndexOf(')');
					string substring = line.Substring(1, i - 1);
					if (!symbolTable.ContainsKey(substring))
						symbolTable.Add(substring, lineNum);
					return null;
				}
				int j = line.IndexOf("//");
				if (j != -1) {
					line = line.Substring(0, j);
				}

			return line;
		}

		public void AddVariablesSymbols(string line) {
			if (line[0] == '@' && !Char.IsDigit(line[1])) {
				string substring = line.Substring(1);
				if (!symbolTable.ContainsKey(substring)) {
					symbolTable.Add(substring, freeAddress);
					freeAddress++;
				}
			}
		}

		public string GenerateMachineCode() {
			
			if (instr.type == 1) {

				int compB = 0b0;
				string tmp = instr.a == 0 ? "A" : "M";
				if (instr.comp.Contains('|'))
					compB = 0b010101;
				else if (instr.comp.Contains('&'))
					compB = 0;
				else if (instr.comp == "0" )
					compB = 0b101010;
				else if (instr.comp == "1")
					compB = 0b111111;
				else if (instr.comp == "-1")
					compB = 0b111010;
				else if (instr.comp == "D")
					compB = 0b001100;
				else if (instr.comp == "!D")
					compB = 0b001101;
				else if (instr.comp == "-D")
					compB = 0b001111;
				else if (instr.comp == "D+1")
					compB = 0b011111;
				else if (instr.comp == "D-1")
					compB = 0b001110;

				else if (instr.comp == tmp)
					compB = 0b110000;
				else if (instr.comp == "!" + tmp)
					compB = 0b110001;
				else if (instr.comp == "-"+ tmp)
					compB = 0b110011;
				else if (instr.comp == tmp + "+1")
					compB = 0b110111;
				else if (instr.comp == tmp + "-1")
					compB = 0b110010;
				else if (instr.comp == "D+" + tmp)
					compB = 0b000010;
				else if (instr.comp == "D-" + tmp)
					compB = 0b010011;
				else if (instr.comp == tmp + "-D")
					compB = 0b000111;


				
				int destB = 0b0;
				if (instr.dest != null) {
					if (instr.dest.Contains("M"))
						destB |= 1;
					if (instr.dest.Contains("D"))
						destB |= 1 << 1;
					if (instr.dest.Contains("A"))
						destB |= 1 << 2;
				}
				

				int jumpB = 0b0;
				if (instr.jump == null)
					jumpB = 0b000;
				else if (instr.jump == "JGT")
					jumpB = 0b001;
				else if (instr.jump == "JEQ")
					jumpB = 0b010;
				else if (instr.jump == "JGE")
					jumpB = 0b011;
				else if (instr.jump == "JLT")
					jumpB = 0b100;
				else if (instr.jump == "JNE")
					jumpB = 0b101;
				else if (instr.jump == "JLE")
					jumpB = 0b110;
				else if (instr.jump == "JMP")
					jumpB = 0b111;


				string CInstr = "111" + instr.a.ToString() + Convert.ToString(compB, 2).PadLeft(6, '0') + Convert.ToString(destB, 2).PadLeft(3, '0') + Convert.ToString(jumpB, 2).PadLeft(3, '0');
				return CInstr;
			}

			string valB = Convert.ToString(instr.value, 2);
			string AInstr = new String('0', 16 - valB.Length) + valB;
			return AInstr;
			
		}
		
	}
}
