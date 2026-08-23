using System;
using Godot.Collections;
namespace NullGarel.Util;

/*
*   2025????????
*   DISCLAIMER: CLANKER GENERATED. POTENTIALLY SLOPPY CODE.
*   AGENT: ChatGPT probably
*/
public class NameGenerator
{
    private Array<string> _nameBeginnings;
    private Array<string> _nameEndings;
    private Array<string> _vowels;
    private Array<string> _simpleConsonants;
    private readonly Random _random;
    private bool _useDictedPatterns;

    private NameGenerator()
    {
        // Default to simple patterns
        _useDictedPatterns = false;

        // Dicted name elements
        _nameBeginnings = [
            // Original names
            "Christ", "Joh", "Will", "Ed", "Rich", "Rob", "Thom", "Jam", "Mich", "Dav", "Paddy", "Jos", "Tom",
    "Alex", "Ben", "Charl", "Fran", "Georg", "Hen", "Jac", "Louis", "Matt", "Nathan", "Mat", "Bell",
    "Pat", "Sam", "Steph", "Tim", "Vict", "Zach", "Luc", "Max", "Osc", "Pete", "Edw", "Ew", "Mac", "Jon",
    "Ann", "Beth", "Carol", "Dian", "Ell", "Em", "Gabri", "Hann", "Isab", "Jess", "Giov", "Mig", "Bern",
    "Kath", "Laur", "Liz", "Mari", "Nic", "Oliv", "Rachel", "Sarah", "Soph", "Victor", "Bruces", "Hamilt",
    "Goph", "Don",
    
    // Expanded - Common English
    "Ad", "Al", "And", "Ant", "Arn", "Ash", "Bart", "Brand", "Brent", "Brett", "Brian", "Cal", "Cam", "Carl",
    "Ced", "Chad", "Chris", "Clay", "Cliff", "Col", "Con", "Craig", "Curt", "Dan", "Dar", "Dean", "Derek",
    "Der", "Doug", "Drew", "Dyl", "Earl", "Eli", "Eric", "Evan", "Finn", "Flor", "For", "Gar", "Gav", "Glen",
    "Grant", "Greg", "Har", "Hay", "Heath", "Hugh", "Hunter", "Ian", "Isaac", "Ivan", "Jack", "Jar", "Jas",
    "Jed", "Jeff", "Jer", "Joel", "John", "Jonah", "Jordan", "Jude", "Jul", "Just", "Keith", "Ken", "Kent",
    "Kevin", "Kurt", "Kyle", "Lance", "Lane", "Lee", "Leo", "Leon", "Liam", "Lincoln", "Logan", "Luke", "Mark",
    "Mart", "Mel", "Mike", "Miles", "Mitch", "Morgan", "Neil", "Noah", "Norm", "Ol", "Owen", "Paul", "Perry",
    "Phil", "Quin", "Ray", "Reed", "Rex", "Rhett", "Rick", "Riley", "Rod", "Roger", "Ross", "Roy", "Ryan",
    "Scott", "Sean", "Seth", "Shane", "Shawn", "Sid", "Simon", "Stan", "Steve", "Stu", "Syl", "Ted", "Terr",
    "Tod", "Tony", "Trent", "Trev", "Trist", "Troy", "Tyl", "Vance", "Vern", "Vic", "Wade", "Walt", "Wayne",
    "Wes", "Xav", "Zane",
    
    // Female names
    "Ab", "Adri", "Agn", "Al", "Ali", "All", "Am", "Amand", "Amel", "Amy", "Andr", "Angel", "An", "Ari",
    "Aud", "Barb", "Beat", "Beck", "Bev", "Bian", "Bon", "Brid", "Brit", "Brooke", "Cam", "Car", "Cass",
    "Cat", "Cath", "Cec", "Chel", "Cher", "Chloe", "Chris", "Clair", "Clar", "Claud", "Con", "Court", "Crystal",
    "Cyn", "Dais", "Daph", "Deb", "Del", "Dest", "Dol", "Don", "Dor", "Eden", "El", "Elean", "Eliz", "Ell",
    "Em", "Emm", "Er", "Est", "Eth", "Eve", "Faith", "Fay", "Fel", "Fran", "Fred", "Gab", "Gail", "Gem",
    "Georg", "Gin", "Glad", "Glenn", "Glor", "Grace", "Gwen", "Heath", "Heid", "Hel", "Hope", "Ida", "Iris",
    "Iv", "Jack", "Jan", "Janet", "Jean", "Jen", "Jess", "Jill", "Jo", "Joc", "Joy", "Joyce", "Jud", "Jul",
    "June", "Kait", "Karen", "Kate", "Kath", "Kat", "Kay", "Kelly", "Kim", "Kris", "Lace", "Lana", "Le",
    "Leah", "Leann", "Len", "Les", "Lil", "Lily", "Lind", "Lisa", "Liv", "Lois", "Lor", "Lou", "Luc", "Lucy",
    "Lynn", "Madd", "Mad", "Mae", "Magg", "Marc", "Marg", "Mar", "May", "Meg", "Mel", "Mer", "Mid", "Mir",
    "Moll", "Mon", "Moon", "Myr", "Naom", "Nat", "Neil", "Nell", "Nicol", "Nora", "Od", "Ol", "Op", "Pam",
    "Paige", "Pearl", "Peg", "Pen", "Phyl", "Pris", "Quinn", "Rach", "Raquel", "Reb", "Reg", "Rhond", "Rit",
    "Rob", "Rosa", "Rose", "Ros", "Rox", "Ruth", "Sab", "Sade", "Sally", "Sam", "Sand", "Sara", "Shan",
    "Sharon", "She", "Shel", "Sher", "Sien", "Sky", "Stac", "Star", "Steph", "Sue", "Summer", "Susan",
    "Syd", "Sylv", "Tamm", "Tay", "Ter", "Tess", "The", "Theod", "Tif", "Tin", "Tracy", "Val", "Van",
    "Vera", "Veron", "Vick", "Viol", "Virg", "Viv", "Wand", "Whit", "Will", "Win", "Wyn", "Xen", "Yvonne",
    "Zoe",
    
    // International/ethnic variations
    "Abd", "Ah", "Al", "Amin", "Amir", "An", "Art", "Bar", "Ben", "Cas", "Dem", "Des", "Di", "Dom", "Eb",
    "El", "Em", "En", "Ezek", "Ezr", "Fab", "Fitz", "Ham", "Han", "Has", "Hass", "Ibr", "Idr", "Is", "Jab",
    "Jal", "Jam", "Jav", "Jay", "Jer", "Jes", "Joa", "Jon", "Jos", "Juan", "Jul", "Jun", "Kam", "Kar", "Kas",
    "Kev", "Khal", "Kris", "Lam", "Laz", "Le", "Lev", "Lin", "Lor", "Lu", "Mal", "Man", "Mar", "Mel", "Men",
    "Mic", "Moh", "Nas", "Nat", "Naz", "Nels", "Nik", "No", "Om", "Or", "Os", "Pab", "Pac", "Pas", "Ped",
    "Pet", "Raf", "Raj", "Ram", "Rash", "Ray", "Ren", "Ric", "Rob", "Rod", "Rom", "Rui", "Sal", "Sam", "San",
    "Seb", "Ser", "Sim", "Sol", "Ste", "Sul", "Tariq", "Th", "Tob", "Tyr", "Umar", "Vlad", "Wes", "Xan", "Yus",
    "Zac", "Zay"
        ];

        _nameEndings = [
            // Original endings
            "neres", "opher", "nathan", "iel", "iam", "ias", "uel", "ard", "ert", "ew", "in", "eph", "el", "lad", "lass",
    "ob", "on", "ory", "uel", "vin", "y", "ty", "dy", "ny", "my", "an", "ord", "bert", "id", "i",
    "ley", "ton", "son", "man", "las", "mas", "rus", "vin", "don", "bell", "loyd", "anni", "rich",
    "a", "ia", "ie", "y", "elle", "ette", "ine", "ana", "ella", "ora", "bush", "field", "land", "borough",
    "issa", "ica", "ena", "ara", "ina", "elle", "anne", "lyn", "rose", "mary", "ace", "ray", "taylor", "essa",
    "ald",
    
    // Expanded - Common English
    "abelle", "abeth", "acia", "acy", "ada", "ade", "aiden", "aine", "air", "ais", "ak", "aker", "ale", "alia",
    "alie", "am", "amond", "amy", "ana", "ance", "and", "ander", "andra", "ane", "angel", "anie", "ann", "antha",
    "anthy", "anton", "any", "arc", "arch", "ard", "aria", "ario", "ark", "arl", "arne", "aro", "aron", "arrel",
    "arry", "arth", "arus", "ary", "as", "ason", "ast", "aster", "athan", "atius", "ato", "aul", "aun", "aust",
    "ax", "ay", "aya", "ayne", "ays", "az", "ball", "bara", "bard", "bass", "bast", "be", "beck", "bel", "bell",
    "ben", "bens", "ber", "berg", "bert", "beth", "bie", "bin", "bird", "born", "bourne", "briel", "bridge",
    "bro", "brook", "burn", "burne", "bus", "by", "ca", "cah", "cal", "can", "car", "carlos", "cas", "cass",
    "caster", "cay", "ce", "cel", "cent", "cer", "ces", "ch", "cha", "chal", "chan", "chard", "chel", "chell",
    "chi", "chia", "chie", "chol", "cia", "cille", "cine", "cio", "cius", "clair", "clay", "cle", "co", "cock",
    "coe", "col", "comb", "come", "con", "cord", "cott", "cox", "cy", "da", "dall", "dan", "dana", "dard",
    "das", "dash", "de", "dee", "del", "dell", "den", "der", "des", "deus", "di", "dia", "dian", "die", "din",
    "dine", "ding", "dith", "dock", "doe", "dom", "don", "dor", "dora", "dos", "dox", "drew", "dry", "dwell",
    "dy", "ean", "eb", "ec", "ece", "eck", "ed", "ee", "een", "eena", "eese", "ef", "egan", "eigh", "eith",
    "el", "ela", "eland", "ele", "elia", "elio", "ell", "ella", "elle", "ello", "elly", "elo", "elph", "eman",
    "en", "ena", "ence", "end", "ene", "enia", "enio", "enna", "enne", "eno", "ent", "enter", "eo", "eph",
    "er", "era", "erel", "eria", "erick", "erie", "erik", "erill", "erine", "erio", "erius", "erman", "ero",
    "eron", "ers", "erson", "ert", "erto", "ery", "es", "esa", "ese", "esh", "eson", "ess", "esta", "este",
    "et", "eta", "eth", "ett", "etta", "ette", "etti", "etto", "ety", "eus", "ev", "ever", "ey", "ez", "fa",
    "fan", "far", "fast", "feld", "fer", "ff", "ffin", "field", "fin", "fine", "fany", "ford", "fort", "fox",
    "fry", "fus", "ga", "gan", "gard", "gate", "gay", "gel", "gem", "gen", "geo", "ger", "gett", "ghan",
    "gia", "gian", "giel", "gio", "gio", "gis", "glen", "go", "gold", "gon", "good", "gor", "gos", "grace",
    "grad", "grand", "grant", "graph", "grave", "gray", "green", "gren", "gress", "grey", "gus", "guy", "ha",
    "hall", "ham", "han", "hand", "hard", "hart", "hat", "head", "heart", "heath", "hed", "heel", "heim",
    "hel", "hell", "hen", "her", "hes", "hew", "hia", "hill", "horn", "house", "hugh", "hunt", "hurst", "i",
    "ia", "iah", "ian", "iana", "ias", "ib", "ic", "ica", "ice", "ick", "ico", "ida", "idel", "idie", "ie",
    "ield", "ien", "ienne", "ier", "ies", "ifa", "ifer", "iff", "ight", "ik", "il", "ila", "ilda", "ile",
    "ilia", "ille", "illo", "illy", "ilou", "ilus", "im", "ima", "ime", "in", "ina", "ine", "ing", "inia",
    "inn", "ino", "io", "ion", "ior", "ios", "ious", "ique", "ira", "ire", "iro", "is", "isa", "ise", "ish",
    "isia", "ison", "iss", "ista", "ito", "ius", "ive", "iver", "ivia", "ix", "iza", "ja", "jack", "jam",
    "jan", "jar", "jas", "jay", "jen", "jo", "john", "jor", "jos", "joy", "jud", "jus", "ka", "kay", "ke",
    "kel", "ken", "kend", "ker", "kes", "key", "ki", "kie", "kil", "kin", "king", "kiss", "kit", "ko", "la",
    "lace", "lah", "lain", "lake", "lam", "lan", "land", "lane", "lar", "las", "lass", "last", "law", "lay",
    "le", "lea", "leaf", "leck", "ledge", "lee", "leen", "leigh", "lene", "leno", "leo", "ler", "les",
    "less", "lest", "let", "ley", "li", "lia", "lian", "lias", "lice", "lick", "lid", "lie", "lier", "light",
    "lik", "lin", "lind", "line", "ling", "lio", "lip", "lis", "lish", "list", "lius", "liv", "ll", "lla",
    "llan", "lland", "lle", "llen", "ller", "lley", "lliam", "llian", "llie", "llins", "llis", "llo", "llow",
    "lly", "lo", "lock", "lod", "log", "lon", "long", "look", "lord", "los", "lot", "lou", "love", "low",
    "lox", "lph", "ls", "lt", "lton", "lus", "ly", "lyn", "lynn", "ma", "mac", "mack", "mad", "mage", "mah",
    "main", "mal", "man", "mand", "mari", "mark", "mart", "mas", "matt", "max", "may", "maz", "me", "med",
    "mel", "men", "mend", "mer", "mers", "mes", "met", "meth", "mett", "mey", "mi", "mia", "miah", "mic",
    "mick", "mie", "mil", "mill", "min", "mine", "ming", "mir", "mis", "mit", "mith", "mo", "mon", "mond",
    "mont", "mond", "mood", "more", "mos", "mour", "mouth", "mus", "my", "na", "nah", "nal", "nan", "nard",
    "nas", "nath", "ne", "nell", "nel", "ner", "nes", "ness", "net", "nette", "ney", "ni", "nia", "nic",
    "nick", "nie", "niel", "nier", "night", "nik", "nil", "nim", "nine", "ning", "nio", "nis", "nish", "nith",
    "nix", "no", "non", "nor", "nos", "not", "now", "nny", "nus", "ny", "o", "ob", "ock", "od", "ode", "oe",
    "of", "off", "oge", "ohn", "ol", "old", "ole", "olin", "oll", "om", "oma", "ome", "on", "ona", "one",
    "onia", "onio", "ony", "oo", "ook", "ool", "oon", "op", "or", "ora", "ord", "ore", "org", "oria", "orie",
    "orio", "orn", "orpe", "orth", "os", "ose", "osh", "oss", "ost", "ot", "ott", "ou", "ough", "our", "ous",
    "out", "ov", "ow", "owe", "owell", "own", "ox", "oy", "oz", "p", "pa", "pal", "pan", "par", "pard",
    "pas", "pat", "paz", "pe", "ped", "pel", "pen", "per", "pers", "pes", "pet", "pey", "ph", "pha", "phe",
    "pher", "phi", "phia", "phie", "phine", "phne", "pho", "phy", "pi", "pia", "pie", "pin", "pine", "ping",
    "pio", "pis", "pit", "pman", "po", "pol", "pont", "pool", "por", "port", "pos", "pot", "pow", "pper",
    "pps", "pres", "pri", "ps", "psey", "pton", "pwell", "py", "quan", "que", "quel", "quin", "r", "ra",
    "rach", "rad", "rael", "rah", "rail", "raine", "ral", "rald", "ram", "ran", "rand", "range", "rant",
    "raph", "rar", "ras", "rash", "rast", "rath", "rau", "raw", "ray", "re", "red", "ree", "reen", "rell",
    "rence", "ren", "rent", "rer", "res", "ress", "ret", "rett", "rey", "ri", "ria", "rian", "rias", "ric",
    "rice", "rick", "rid", "rie", "riel", "rien", "rier", "rietta", "rig", "right", "rik", "ril", "rill",
    "rim", "rin", "rina", "rine", "ring", "rio", "rion", "rios", "ris", "rish", "rist", "rit", "rith", "rix",
    "riz", "ro", "rock", "rod", "roe", "rog", "rol", "roll", "rom", "rome", "ron", "rone", "rong", "rop",
    "ror", "ros", "rose", "ross", "rot", "rou", "rough", "round", "rouse", "row", "rox", "roy", "rs", "rt",
    "rta", "rte", "rth", "rto", "rtz", "ru", "rub", "rud", "ruff", "rul", "rum", "run", "rup", "rus", "rush",
    "rut", "ruth", "ry", "ryan", "s", "sa", "sac", "sah", "sal", "sam", "san", "sandro", "sar", "sara",
    "saw", "say", "scott", "se", "sea", "sel", "sen", "ser", "set", "sey", "sh", "sha", "shaw", "she",
    "shell", "shel", "sher", "shua", "si", "sia", "siah", "sias", "sica", "sie", "sier", "sig", "sil",
    "sile", "sill", "sim", "sin", "sine", "sing", "sio", "sir", "sis", "sius", "ska", "skey", "sky", "slaw",
    "sley", "sly", "smith", "so", "sol", "son", "sor", "sper", "ss", "ssell", "ssia", "sta", "stan", "star",
    "stead", "stell", "sten", "ster", "stian", "stin", "ston", "stopher", "sworth", "sy", "t", "ta", "tch",
    "te", "tel", "ten", "ter", "ters", "tes", "tez", "th", "tha", "than", "thel", "ther", "thia", "thie",
    "tho", "thon", "thor", "thony", "thus", "thy", "ti", "tia", "tian", "tias", "tice", "tie", "tien",
    "tier", "tina", "tine", "ting", "tio", "tion", "tish", "tius", "tiz", "to", "tob", "tod", "tom", "ton",
    "tone", "top", "tor", "toria", "torie", "torio", "torr", "tory", "tos", "tosh", "tram", "tran", "tre",
    "trel", "tress", "trey", "tri", "trick", "trix", "tro", "tron", "try", "ts", "tt", "tte", "tter", "tty",
    "tus", "ty", "tz", "u", "ual", "uan", "ub", "uca", "uce", "uck", "ud", "uel", "uff", "ugh", "ui", "uk",
    "ul", "ula", "uld", "ule", "ulia", "ulie", "ulio", "ull", "ullo", "ulou", "ulph", "ult", "um", "un",
    "una", "une", "ung", "uo", "up", "ur", "ura", "urd", "ure", "urg", "uri", "urio", "urk", "url", "urn",
    "uro", "urs", "urt", "us", "usa", "use", "ush", "uson", "ust", "ut", "uth", "utt", "uz", "v", "va",
    "van", "var", "vas", "ve", "ved", "vel", "ven", "ver", "vers", "ves", "vey", "vi", "via", "vian", "vic",
    "vich", "vie", "vier", "vin", "vine", "ving", "vio", "vir", "vis", "vius", "von", "vor", "vy", "w", "wa",
    "wald", "walk", "wall", "wan", "ward", "ware", "water", "way", "well", "wen", "wer", "wers", "wes",
    "west", "white", "wick", "wid", "wig", "win", "wind", "wing", "winn", "wise", "witch", "witz", "wolf",
    "wood", "worth", "wright", "ws", "x", "xa", "xander", "xie", "xton", "xy", "y", "ya", "yan", "yard",
    "yas", "ye", "yer", "yl", "ym", "yn", "yne", "yo", "yon", "yor", "you", "young", "ys", "yson", "yt",
    "yu", "z", "za", "zac", "zah", "zan", "zar", "ze", "zel", "zer", "zi", "zia", "zie", "zio", "zipper",
    "zle", "zo", "zon", "zra", "zzer"
        ];

        _vowels = ["a", "e", "i", "o", "u"];
        _simpleConsonants = ["b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "r", "s", "t", "v", "w", "z"];

        _random = new();
    }

    public static NameGenerator Create()
    {
        return new NameGenerator();
    }

    // Fluent builder methods
    public NameGenerator UseDictedPatterns()
    {
        _useDictedPatterns = true;
        return this;
    }

    public NameGenerator UseSimplePatterns()
    {
        _useDictedPatterns = false;
        return this;
    }

    public NameGenerator WithNameBeginnings(Array<string> beginnings)
    {
        _nameBeginnings = beginnings;
        return this;
    }

    public NameGenerator WithNameEndings(Array<string> endings)
    {
        _nameEndings = endings;
        return this;
    }

    public NameGenerator WithVowels(Array<string> vowels)
    {
        this._vowels = vowels;
        return this;
    }

    public NameGenerator WithConsonants(Array<string> consonants)
    {
        _simpleConsonants = consonants;
        return this;
    }

    private string GenerateDictedName()
    {
        if (_random.Next(5) > 0)
        {
            string beginning = _nameBeginnings[_random.Next(_nameBeginnings.Count)];
            string ending = _nameEndings[_random.Next(_nameEndings.Count)];

            if (IsConsonant(beginning[beginning.Length - 1]) && IsConsonant(ending[0]))
            {
                string vowelBridge = _vowels[_random.Next(_vowels.Count)];
                return beginning + vowelBridge + ending;
            }

            return beginning + ending;
        }
        else
        {
            string firstSyllable = GenerateSyllable("CV");
            string secondSyllable = GenerateSyllable("CVC");
            return firstSyllable + secondSyllable;
        }
    }

    private string GenerateSimpleName()
    {
        int syllables = _random.Next(2, 6);
        string name = "";
        for (int i = 0; i < syllables; i++)
        {
            name += GenerateSyllable("CV");
        }
        return name;
    }

    private string GenerateSyllable(string pattern)
    {
        string syllable = "";
        foreach (char c in pattern)
        {
            if (c == 'C')
            {
                syllable += _simpleConsonants[_random.Next(_simpleConsonants.Count)];
            }
            else if (c == 'V')
            {
                syllable += _vowels[_random.Next(_vowels.Count)];
            }
        }
        return syllable;
    }

    private bool IsConsonant(char c)
    {
        return !"aeiouAEIOU".Contains(c.ToString());
    }

    public string GenerateName()
    {
        string name = _useDictedPatterns ? GenerateDictedName() : GenerateSimpleName();
        return char.ToUpper(name[0]) + name.Substring(1);
    }
}