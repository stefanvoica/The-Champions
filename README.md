# Proiect MDS 2025 – OnlineCleaningShop

## Autori
- Voica Ștefan-Alexandru (251)
- Ancuța Theodor Constantin (251)
- Moloceniuc Albert (252)
- Cristescu Ciprian (252)
- Coman Irina (233)

## Prezentare
  Proiectul propune dezvoltarea unei platforme complete de comerț online, care integrează
funcționalități esențiale pentru gestionarea utilizatorilor, produselor, comenzilor și a proceselor
administrative. Scopul este crearea unei experiențe fluide și sigure atât pentru cumpărători, cât și
pentru colaboratori și administratori.
  Aplicația va permite utilizatorilor să creeze conturi, să se autentifice în siguranță folosind
verificare în doi pași și să navigheze printre produse organizate pe categorii. Vor putea adăuga
produse în coș, plasa comenzi, alege metode de livrare și folosi coduri promoționale. După
finalizarea comenzii, utilizatorii vor primi automat factura prin e-mail.
Colaboratorii vor avea posibilitatea de a adăuga și gestiona produse, iar administratorii vor
aproba aceste produse și vor avea control deplin asupra conținutului din platformă, inclusiv
categorii, recenzii și drepturile utilizatorilor.
  Platforma va include funcționalități moderne precum sistem de recenzii și evaluări, motor de
căutare performant, sortare după preț și rating si chatbox pentru suport în timp real care va replica toate funcțiile site-ului.
Securitatea și scalabilitatea sunt priorități-cheie, astfel încât proiectul este gândit pentru a putea fi
extins ușor și integrat cu servicii externe (plăți online, notificări SMS/email, livrări automate
etc.).
Demo video: [AICI](#)


##  Procesul de dezvoltare software

###  1. User Stories și Backlog
Ca vizitator, vreau sa ma pot inregistra pentru a avea acces la functionalitati suplimentare.
Ca utilizator, vreau sa ma pot autentifica in 2 pasi, primind un mail/sms cu un cod de validare.
Ca utilizator colaborator, vreau sa pot adauga, edita si sterge produse in magazin, care sa fie apoi aprobate sau respinse de administrator.
Ca administrator, vreau sa pot adauga, edita si sterge categorii de produse pentru a organiza magazinul.
Ca administrator, vreau sa pot schimba drepturile de acces ale utilizatorilor.
Ca utilizator, vreau sa pot lasa un rating si un review text pentru produsele achizitionate.
Ca utilizator neinregistrat, vreau doar sa pot vizualiza produsele si comentariile, fara a putea adauga produse in cos si lasa recenzii.
Ca utilizator inregistrat, vreau sa pot adauga produse in cos si sa plasez comenzi.
Ca utilizator, vreau sa pot cauta produse dupa denumire sau fragmente din numele acestora.
Ca utilizator, vreau sa pot sorta produsele dupa pret sau rating, crescator sau descrescator.
Ca administrator, vreau sa pot edita si sterge produse si comentarii.
Ca utilizator, vreau ca la finalul comenzii sa fiu redirectionat catre o pagina de plati online.
Ca utilizator, vreau sa primesc mail cu factura autogenerata in urma comenzii efectuate in magazin.
Ca utilizator, vreau sa pot alege din mai multe metode de livrare (ridicare din depozit, easybox, livrare prin curier).
Ca utilizator, vreau sa beneficiez de transport gratuit sau reduceri începând cu o anumita suma  a produselor din cosul de cumparaturi.
Ca utilizator, vreau sa am posibilitatea de a ma abona la newsletter pentru a ma informa din cele mai recente surse si a descoperi cele mai noi reduceri.
Ca utilizator, vrea sa pot folosi coduri promotionale in comenzile mele.
Ca utilizator, vreau să am acces la un HelpAI care să includă o opțiune de chat pentru a primi asistență rapidă și clarificări legate de produse, comenzi și funcționalitățile platformei.

###  2. Diagrame UML / Workflow
- Diagrama conceptuală
  
- Diagrama entitate-relație

###  3. Source control folosind Git
Gestionarea codului sursă se realizează prin GitHub, folosind cele mai bune practici:

#### Branch Creation și Workflow
Folosim modelul GitFlow, cu următoarele ramuri principale:
- main – cod stabil, gata de producție
- develop – integrare continuă a funcționalităților
- feature/nume-funcționalitate – dezvoltare de noi funcții
- bugfix/nume-bug – rezolvarea bug-urilor identificate
- hotfix/nume-problema – intervenții rapide în producție

#### Merge și Rebase
Modificările sunt integrate prin Pull Requests, cu verificare și aprobare în echipă

Se utilizează rebase pentru menținerea unui istoric liniar și curat

Merge se face doar după code review și rezolvarea tuturor comentariilor

#### Commits
Proiectul conține peste 85 de commit-uri

Se respectă convențiile de denumire:
- feat: pentru funcționalități noi
- fix: pentru rezolvări de bug-uri
- refactor: pentru restructurarea codului
- docs: pentru documentație


###  4. Teste automate
- 

###  5. Raportare bug & rezolvare folosind pull request-uri
![image](https://github.com/user-attachments/assets/98cb4750-8e3d-4db1-8573-34961a583a4b)

Gestionarea bug-urilor se realizează prin:

Identificarea și Raportarea - Utilizăm sistemul de Issues din GitHub pentru a documenta bug-urile descoperite

Analiza și Prioritizarea - Evaluăm impactul și urgența fiecărui bug pentru a stabili prioritatea

Rezolvarea - Creăm branch-uri dedicate pentru rezolvarea bug-urilor (bugfix/nume-bug)

Pull Request și Review - Soluțiile sunt verificate prin PR-uri și code review

Testare - Verificăm că rezolvarea nu afectează alte funcționalități

Integrare - După aprobare, modificările sunt integrate în branch-ul principal


###  6. Comentarii cod & code standards
#### Code Standards
Folosim StyleCop pentru C# și ESLint pentru JavaScript/TypeScript
Respectăm convențiile de denumire specifice fiecărui limbaj
Aplicăm principiile SOLID și Clean Code
#### Comentarii Cod
Fiecare clasă și metodă publică are comentarii XML pentru documentație
Algoritmii complexi sunt documentați cu explicații detaliate
Folosim comentarii pentru a explica deciziile de arhitectură și design
###  7. Design Patterns
În cadrul proiectului am utilizat mai multe pattern-uri standard de proiectare:

1. Dependency Injection (DI)
Aplicat extensiv în Program.cs, unde serviciile sunt înregistrate și injectate în controlere sau servicii:

AddScoped<IBraintreeService, BraintreeService>()

AddTransient<EmailService>()

AddSingleton<GeminiService>()

2. Interface-based Programming
Interfața IBraintreeService este folosită pentru a defini contractul serviciului de plată, implementat de BraintreeService, permițând testabilitate și extensibilitate crescută.

3. Factory Pattern (prin configurație)
În BraintreeService, obiectul BraintreeGateway este creat pe baza valorilor de configurare, funcționând ca o instanțiere dinamică în funcție de mediu, ceea ce reflectă un pattern de tip Factory.

4. Singleton Pattern
Serviciile statice precum GeminiService sunt înregistrate ca singleton-uri, pentru a reutiliza aceeași instanță pe durata aplicației și a reduce consumul de resurse.

5. Seeder Pattern (Custom Initialization)
Clasa SeedData aplică un pattern de tip initializer, folosit pentru a popula baza de date cu roluri și utilizatori inițiali într-un mod controlat și repetabil.

Aceste pattern-uri contribuie la o arhitectură modulară, scalabilă și ușor de întreținut.

###  8. Prompt Engineering & AI Tools (2p)

  La una dintre cerinte am implementat un chat box in cadrul caruia am folosit un API de Gemini, pe care
utilizatorul il poate intreba diverse detalii despre produse (ex: o comparatie intre produse, care este cel
mai bun)
  - ChatGPT pentru clarificări de sintaxă și optimizări
  - Documentarea prompturilor este disponibilă [aici](#)

