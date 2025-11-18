# WinGet Studio Data & Privacy
## Overview
WinGet Studio diagnostic data is completely optional for users and is off by default in v0.100.318.0 and beyond. Our team believes in transparency and trust. As WinGet Studio is open source, all of our diagnostic data events are in the codebase.

Additionally, this document aims to list each diagnostic data event individually and describe their purpose clearly.

For more information, please read the [Microsoft privacy statement](https://privacy.microsoft.com/privacystatement). 

## What does WinGet Studio collect?

1. **Usage**: Understanding usage and frequency rates for utilities and settings helps us make decisions on where to focus our time and energy.
2. **Stability**: Monitoring bugs and system crashes, as well as analyzing GitHub issue reports, assists us in prioritizing the most urgent issues.
3. **Performance**: Assessing the performance of WinGet Studio features to load and execute gives us an understanding of what surfaces are causing slowdowns. This supports our commitment to providing you with tools that are both speedy and effective.

## Transparency and Public Sharing
As much as possible, we aim to share the results of diagnostic data publicly.

We hope this document provides clarity on why and how we collect diagnostic data to improve WinGet Studio for our users. If you have any questions or concerns, please feel free to reach out to us.

Thank you for using WinGet Studio!

## List of Diagnostic Data Events

_If you want to find diagnostic data events in the source code, this link will be good starting point._
- [Events code search](https://github.com/search?q=repo%3Amicrosoft/winget-studio%20EventBase&type=code)

### General
<table style="width:100%">
  <tr>
    <th>Event Name</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>Microsoft.WinGetStudio.NavigatedToPageEvent</td>
    <td>Logs page navigation information.</td>
  </tr>
</table>
