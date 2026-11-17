// Contenu pédagogique. Rédigé par une IA : à faire relire par un locuteur natif.
// ar = alphabet arabe (envoyé à la synthèse vocale), tr = translittération, fr = traduction.
// Translittération : 2 = coup de glotte, 3 = ع, 7 = ح, kh = خ, gh = غ, sh = ش.

export const LESSONS = [
  {
    id: 'salutations',
    title: 'Salutations',
    items: [
      { id: 'sal-1', ar: 'مرحبا', tr: 'mar7aba', fr: 'Bonjour / Salut' },
      { id: 'sal-2', ar: 'أهلا وسهلا', tr: 'ahla w sahla', fr: 'Bienvenue' },
      { id: 'sal-3', ar: 'صباح الخير', tr: 'saba7 el-kheir', fr: 'Bonjour (le matin)' },
      { id: 'sal-4', ar: 'صباح النور', tr: 'saba7 en-nour', fr: 'Bonjour (réponse à « saba7 el-kheir »)' },
      { id: 'sal-5', ar: 'مسا الخير', tr: 'masa el-kheir', fr: 'Bonsoir' },
      { id: 'sal-6', ar: 'كيفك؟', tr: 'kifak? / kifik?', fr: 'Comment vas-tu ?', note: 'kifak à un homme, kifik à une femme' },
      { id: 'sal-7', ar: 'منيح، الحمدلله', tr: 'mnee7, el-7amdella', fr: 'Bien, Dieu merci', note: 'une femme dit mnee7a' },
      { id: 'sal-8', ar: 'شو أخبارك؟', tr: 'shu akhbarak?', fr: 'Quoi de neuf ?', note: 'akhbarik à une femme' },
      { id: 'sal-9', ar: 'مع السلامة', tr: 'ma3 es-salemeh', fr: 'Au revoir' },
      { id: 'sal-10', ar: 'بشوفك بعدين', tr: 'bshoufak ba3den', fr: 'À plus tard', note: 'bshoufik à une femme' },
      { id: 'sal-11', ar: 'تصبح على خير', tr: 'tosba7 3ala kheir', fr: 'Bonne nuit', note: 'tosba7i à une femme' },
    ],
  },
  {
    id: 'politesse',
    title: 'Politesse',
    items: [
      { id: 'pol-1', ar: 'شكراً', tr: 'shukran', fr: 'Merci' },
      { id: 'pol-2', ar: 'يسلمو', tr: 'yislamo', fr: 'Merci (familier)', note: 'littéralement « que tes mains soient préservées »' },
      { id: 'pol-3', ar: 'عفواً', tr: '3afwan', fr: 'De rien / Pardon' },
      { id: 'pol-4', ar: 'تكرم', tr: 'tikram', fr: 'Avec plaisir', note: 'tikrami à une femme' },
      { id: 'pol-5', ar: 'إذا بتريد', tr: 'iza betreed', fr: 'S’il te plaît', note: 'littéralement « si tu veux » ; betreede à une femme' },
      { id: 'pol-6', ar: 'عن إذنك', tr: '3an iznak', fr: 'Excuse-moi (pour passer)', note: 'iznik à une femme' },
      { id: 'pol-7', ar: 'آسف', tr: 'eesif', fr: 'Désolé', note: 'une femme dit eesfeh' },
      { id: 'pol-8', ar: 'ولا يهمك', tr: 'wala yhemmak', fr: 'Ne t’inquiète pas', note: 'yhemmik à une femme' },
      { id: 'pol-9', ar: 'إيه', tr: 'eh', fr: 'Oui' },
      { id: 'pol-10', ar: 'لأ', tr: 'la2', fr: 'Non' },
      { id: 'pol-11', ar: 'ماشي', tr: 'meshe', fr: 'D’accord' },
      { id: 'pol-12', ar: 'أكيد', tr: 'akeed', fr: 'Bien sûr' },
    ],
  },
  {
    id: 'presentation',
    title: 'Se présenter',
    items: [
      { id: 'pre-1', ar: 'شو اسمك؟', tr: 'shu ismak?', fr: 'Comment t’appelles-tu ?', note: 'ismik à une femme' },
      { id: 'pre-2', ar: 'اسمي سامي', tr: 'ismi Sami', fr: 'Je m’appelle Sami' },
      { id: 'pre-3', ar: 'تشرفنا', tr: 'tsharrafna', fr: 'Enchanté' },
      { id: 'pre-4', ar: 'من وين إنت؟', tr: 'min wen enta?', fr: 'D’où viens-tu ?', note: 'ente à une femme' },
      { id: 'pre-5', ar: 'أنا من فرنسا', tr: 'ana min fransa', fr: 'Je viens de France' },
      { id: 'pre-6', ar: 'عم بتعلم لبناني', tr: '3am bet3allam lebnene', fr: 'J’apprends le libanais' },
      { id: 'pre-7', ar: 'بحكي لبناني شوي', tr: 'be7ke lebnene shway', fr: 'Je parle un peu libanais' },
      { id: 'pre-8', ar: 'ما فهمت', tr: 'ma fhemet', fr: 'Je n’ai pas compris' },
      { id: 'pre-9', ar: 'فيك تعيد؟', tr: 'fik t3eed?', fr: 'Peux-tu répéter ?', note: 'fiki t3eede à une femme' },
      { id: 'pre-10', ar: 'شوي شوي، إذا بتريد', tr: 'shway shway, iza betreed', fr: 'Doucement, s’il te plaît' },
      { id: 'pre-11', ar: 'شو يعني؟', tr: 'shu ya3ne?', fr: 'Qu’est-ce que ça veut dire ?' },
    ],
  },
  {
    id: 'expressions',
    title: 'Expressions de tous les jours',
    items: [
      { id: 'exp-1', ar: 'يلا', tr: 'yalla', fr: 'Allez / On y va' },
      { id: 'exp-2', ar: 'خلص', tr: 'khalas', fr: 'C’est bon / Ça suffit / Fini' },
      { id: 'exp-3', ar: 'يعني', tr: 'ya3ne', fr: 'C’est-à-dire / Genre' },
      { id: 'exp-4', ar: 'إن شاء الله', tr: 'inshalla', fr: 'Si Dieu le veut / J’espère' },
      { id: 'exp-5', ar: 'ما في مشكلة', tr: 'ma fi meshkleh', fr: 'Pas de problème' },
      { id: 'exp-6', ar: 'شو في ما في؟', tr: 'shu fi ma fi?', fr: 'Quoi de neuf ?', note: 'littéralement « qu’y a-t-il, que n’y a-t-il pas ? »' },
      { id: 'exp-7', ar: 'عن جد؟', tr: '3an jadd?', fr: 'Sérieusement ?' },
      { id: 'exp-8', ar: 'صحتين', tr: 'sa7ten', fr: 'Bon appétit', note: 'littéralement « deux santés »' },
      { id: 'exp-9', ar: 'على قلبك', tr: '3ala 2albak', fr: 'Merci (réponse à « sa7ten »)', note: '2albik à une femme' },
      { id: 'exp-10', ar: 'حبيبي', tr: '7abibi', fr: 'Mon cher / Mon ami', note: 'à une femme : 7abibte' },
      { id: 'exp-11', ar: 'كتير منيح', tr: 'kteer mnee7', fr: 'Très bien' },
      { id: 'exp-12', ar: 'هلق', tr: 'halla2', fr: 'Maintenant' },
      { id: 'exp-13', ar: 'بكرا', tr: 'bukra', fr: 'Demain' },
      { id: 'exp-14', ar: 'يعطيك العافية', tr: 'ya3tik el-3afyeh', fr: 'Bon courage', note: 'se dit à quelqu’un qui travaille' },
      { id: 'exp-15', ar: 'الله يعافيك', tr: 'alla y3afik', fr: 'Merci (réponse à « ya3tik el-3afyeh »)' },
    ],
  },
  {
    id: 'cafe',
    title: 'Au café et au restaurant',
    items: [
      { id: 'caf-1', ar: 'بدي قهوة، إذا بتريد', tr: 'badde 2ahweh, iza betreed', fr: 'Je voudrais un café, s’il te plaît' },
      { id: 'caf-2', ar: 'بدي مي', tr: 'badde may', fr: 'Je veux de l’eau' },
      { id: 'caf-3', ar: 'ما بدي', tr: 'ma badde', fr: 'Je ne veux pas' },
      { id: 'caf-4', ar: 'شو في عندكن؟', tr: 'shu fi 3andkon?', fr: 'Qu’est-ce que vous avez ?' },
      { id: 'caf-5', ar: 'الحساب، إذا بتريد', tr: 'el-7seb, iza betreed', fr: 'L’addition, s’il vous plaît' },
      { id: 'caf-6', ar: 'قديش؟', tr: '2addesh?', fr: 'Combien ?' },
      { id: 'caf-7', ar: 'طيب كتير', tr: 'tayyeb kteer', fr: 'C’est très bon' },
      { id: 'caf-8', ar: 'أنا جوعان', tr: 'ana ju3an', fr: 'J’ai faim', note: 'une femme dit ju3aneh' },
      { id: 'caf-9', ar: 'بلا سكر', tr: 'bala sekkar', fr: 'Sans sucre' },
      { id: 'caf-10', ar: 'كمان واحد', tr: 'kamen wa7ad', fr: 'Encore un' },
    ],
  },
  {
    id: 'chiffres',
    title: 'Chiffres de 1 à 10',
    items: [
      { id: 'chi-1', ar: 'واحد', tr: 'wa7ad', fr: 'Un' },
      { id: 'chi-2', ar: 'تنين', tr: 'tnen', fr: 'Deux' },
      { id: 'chi-3', ar: 'تلاتة', tr: 'tleteh', fr: 'Trois' },
      { id: 'chi-4', ar: 'أربعة', tr: 'arb3a', fr: 'Quatre' },
      { id: 'chi-5', ar: 'خمسة', tr: 'khamseh', fr: 'Cinq' },
      { id: 'chi-6', ar: 'ستة', tr: 'setteh', fr: 'Six' },
      { id: 'chi-7', ar: 'سبعة', tr: 'sab3a', fr: 'Sept' },
      { id: 'chi-8', ar: 'تمانة', tr: 'tmeneh', fr: 'Huit' },
      { id: 'chi-9', ar: 'تسعة', tr: 'tes3a', fr: 'Neuf' },
      { id: 'chi-10', ar: 'عشرة', tr: '3ashra', fr: 'Dix' },
    ],
  },
];

export const DIALOGUES = [
  {
    id: 'rencontre',
    title: 'Faire connaissance',
    lines: [
      { who: 'A', ar: 'مرحبا، كيفك؟', tr: 'mar7aba, kifak?', fr: 'Salut, comment vas-tu ?' },
      { who: 'B', ar: 'منيح، الحمدلله. وإنت؟', tr: 'mnee7, el-7amdella. w enta?', fr: 'Bien, Dieu merci. Et toi ?' },
      { who: 'A', ar: 'ماشي الحال. شو اسمك؟', tr: 'meshe l-7al. shu ismak?', fr: 'Ça va. Comment t’appelles-tu ?' },
      { who: 'B', ar: 'اسمي سامي. وإنت؟', tr: 'ismi Sami. w enta?', fr: 'Je m’appelle Sami. Et toi ?' },
      { who: 'A', ar: 'اسمي كريم. تشرفنا', tr: 'ismi Karim. tsharrafna', fr: 'Je m’appelle Karim. Enchanté.' },
      { who: 'B', ar: 'تشرفنا فيك', tr: 'tsharrafna fik', fr: 'Enchanté également.' },
    ],
  },
  {
    id: 'au-cafe',
    title: 'Commander un café',
    lines: [
      { who: 'A', ar: 'مرحبا، شو بتحب تشرب؟', tr: 'mar7aba, shu bet7ebb teshrab?', fr: 'Bonjour, que veux-tu boire ?' },
      { who: 'B', ar: 'بدي قهوة بلا سكر، إذا بتريد', tr: 'badde 2ahweh bala sekkar, iza betreed', fr: 'Je voudrais un café sans sucre, s’il te plaît.' },
      { who: 'A', ar: 'تكرم. بدك شي تاني؟', tr: 'tikram. baddak shi tene?', fr: 'Avec plaisir. Tu veux autre chose ?' },
      { who: 'B', ar: 'لأ، شكراً. قديش الحساب؟', tr: 'la2, shukran. 2addesh el-7seb?', fr: 'Non, merci. C’est combien ?' },
      { who: 'A', ar: 'خمسة دولار', tr: 'khamseh dolar', fr: 'Cinq dollars.' },
      { who: 'B', ar: 'تفضل. يسلمو', tr: 'tfaddal. yislamo', fr: 'Tiens. Merci.' },
    ],
  },
  {
    id: 'j-apprends',
    title: 'J’apprends le libanais',
    lines: [
      { who: 'A', ar: 'من وين إنت؟', tr: 'min wen enta?', fr: 'D’où viens-tu ?' },
      { who: 'B', ar: 'أنا من فرنسا، بس عم بتعلم لبناني', tr: 'ana min fransa, bass 3am bet3allam lebnene', fr: 'Je viens de France, mais j’apprends le libanais.' },
      { who: 'A', ar: 'برافو! بتحكي منيح', tr: 'bravo! bte7ke mnee7', fr: 'Bravo ! Tu parles bien.' },
      { who: 'B', ar: 'شكراً، بس بحكي شوي شوي', tr: 'shukran, bass be7ke shway shway', fr: 'Merci, mais je parle tout doucement.' },
      { who: 'A', ar: 'ولا يهمك، يلا منحكي', tr: 'wala yhemmak, yalla mne7ke', fr: 'Ne t’inquiète pas, allez, on parle.' },
    ],
  },
];

export const ALL_ITEMS = LESSONS.flatMap((l) => l.items);
export const itemById = (id) => ALL_ITEMS.find((i) => i.id === id);
