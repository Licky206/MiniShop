### Bezbednosni pregled — OWASP
- Sirovi SQL: nije pronađena konkatenacija SQL upita.
- Entiteti kao ulazni modeli: nisu pronađeni direktni Product/Order/Category modeli u akcijama.
- CORS: `AllowAnyOrigin` nije pronađen.
- Tajne: lozinke/tajne nisu pronađene u repozitorijumu.
- JWT ključ: produkcioni ključ nije hardkodovan; koristi konfiguraciju/User Secrets.
- Autorizacija: pristup tuđoj narudžbenici zaštićen politikom `SameOwner` i vraća 403.
- DTO modeli: ne prihvataju `PasswordHash`, `UserId` ili `RowVersion` bez opravdanog razloga.
- Production greške: vraćaju ProblemDetails bez stack trace-a.