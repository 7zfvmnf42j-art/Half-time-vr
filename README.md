# Half-time-vr
New football game

const HALFTIME_BADGES = {
  creator: [
    { name: "Copper", views: 500, color: "#B87333" },
    { name: "Bronze", views: 1000, color: "#CD7F32" },
    { name: "Silver", views: 5000, color: "#CBD5E1" },
    { name: "Gold", views: 15000, color: "#FACC15" },
    { name: "Platinum", views: 30000, color: "#5EEAD4" },
    { name: "Diamond", views: 50000, color: "#38BDF8" },
    { name: "Ruby", views: 75000, color: "#FB405E" },
    { name: "Emerald", views: 100000, color: "#4ADE80" },
    { name: "Elite", views: 150000, color: "#C084FC" },
    { name: "Master", views: 250000, color: "#818CF8" },
    { name: "Legendary", views: 500000, color: "#FB923C" },
    { name: "Icon", views: 1000000, color: "#FFD700" }
  ],

  leagues: [
    "Academy",
    "Division 5",
    "Division 4",
    "Division 3",
    "Division 2",
    "Division 1",
    "Championship",
    "Premier",
    "Elite",
    "World League"
  ],

  exclusive: [
    "Verified",
    "OG",
    "Founder",
    "Beta Tester",
    "VIP",
    "Partner",
    "Bug Hunter",
    "Owner's Choice",
    "One of One"
  ],

  staff: [
    "Owner",
    "Co-Owner",
    "Developer",
    "Admin",
    "Moderator",
    "Trial Moderator",
    "Support"
  ]
};

function getCreatorRank(views, platform = "CC") {
  const rank = [...HALFTIME_BADGES.creator]
    .reverse()
    .find(badge => views >= badge.views);

  if (!rank) return null;

  return {
    name: rank.name + " " + platform,
    color: rank.color,
    views: rank.views
  };
}

function createVerifiedBadge() {
  return `
    <svg viewBox="0 0 120 120"
      width="32" height="32">
      <path
        d="M60 8 L110 56 L60 108 L10 56Z"
        fill="#168BFF"
        stroke="#70EFFF"
        stroke-width="4"
        stroke-linejoin="round"/>
      <path
        d="M29 55 L42 44 L56 60
           L86 29 L79 57 L53 85Z"
        fill="white"/>
    </svg>
  `;
}
