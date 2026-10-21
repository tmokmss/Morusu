// 符号は "0" = dah, "1" = dit の並び
const table: Record<string, string> = {
  "0": "T", "1": "E",
  "00": "M", "01": "N", "10": "A", "11": "I",
  "000": "O", "001": "G", "010": "K", "011": "D",
  "100": "W", "101": "R", "110": "U", "111": "S",
  "0010": "Q", "0011": "Z", "0100": "Y", "0101": "C",
  "0110": "X", "0111": "B", "1000": "J", "1001": "P",
  "1011": "L", "1101": "F", "1110": "V", "1111": "H",
  "10000": "1", "11000": "2", "11100": "3", "11110": "4", "11111": "5",
  "01111": "6", "00111": "7", "00011": "8", "00001": "9", "00000": "0",
  "110011": "?", "01101": "/", "101010": ".", "001100": ",",
  "011110": "-", "100101": "@", "01001": "(", "010010": ")",
  "11111111": "x",
};

const reverse = new Map(Object.entries(table).map(([code, letter]) => [letter, code]));

export function decode(code: string): string {
  return table[code] ?? " ";
}

export function encode(letter: string): string | undefined {
  return reverse.get(letter);
}

export function toMarks(code: string): string {
  return [...code].map((c) => (c === "0" ? "―" : "・")).join(" ");
}
