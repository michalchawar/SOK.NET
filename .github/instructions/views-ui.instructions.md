---
applyTo: '**/*.cshtml'
---
W warstwie web w prostszych widokach staramy się ograniczyć do używania ViewModeli i bezpośredniego podejścia MVC, ale w bardziej zaawansowanych panelach, gdzie potrzebujemy więcej interaktywności, stosujemy VueJS na dwa sposoby - albo tylko rozwijamy client-side interaktywność formularza, który i tak zostanie wysłany do konkretnej akcji MVC, albo w pełni implementujemy wszystkie rzeczy i komunikujemy się z aplikacją przez kontrolery API (w najbardziej zaawansowanych przypadkach).

Jeśli chodzi o UI to używany jest TailwindCSS v4 w połączeniu z biblioteką DaisyUI. Zwróć uwagę, że nie ma tam czegoś takiego jak m.in. "form-control", "label-text", czy "input-bordered".